using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using ThAmCo.WebApi.Data;
using ThAmCo.WebApi.Services;
using ThAmCo.WebApi.Middleware;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Log the connection string (useful for debugging, but avoid logging passwords in production)
        Console.WriteLine("USING CONNECTION: " + builder.Configuration.GetConnectionString("DefaultConnection"));

        // Add services to the container
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();

        // Configure Swagger with JWT support
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "ThAmCo API", Version = "v1" });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        // Configure Entity Framework with SQL Server connection string
        builder.Services.AddDbContext<ThAmCoContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Configure JWT Authentication
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            });

        builder.Services.AddAuthorization();

        // Register application services
        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddScoped<ICustomerService, CustomerService>();
        builder.Services.AddScoped<IOrderService, OrderService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<ISupplierService, SupplierService>();
        builder.Services.AddHttpClient<SupplierService>();

        // Register background services
        builder.Services.AddHostedService<StockUpdateService>();
        builder.Services.AddHostedService<ProductCatalogUpdateService>();

        // Configure CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigin", policy =>
            {
                var frontendUrl = builder.Configuration["FrontendUrl"];
                if (!string.IsNullOrEmpty(frontendUrl))
                {
                    policy.WithOrigins(frontendUrl)
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                }
                else
                {
                    // Fallback for local development or if FrontendUrl is not set
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                }
            });
        });

        builder.Services.AddHealthChecks();

        var app = builder.Build();

        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();

            // In development, listen on localhost:5000
            app.Urls.Clear();
            app.Urls.Add("http://localhost:5000");
            Console.WriteLine("Development environment detected, hosting on http://localhost:5000");
        }
        else
        {
            // In production (Azure), do NOT override URLs so Azure manages it
            Console.WriteLine("Production environment detected, using default hosting URLs");
        }

        app.UseCors("AllowSpecificOrigin");

        // Custom middleware
        app.UseMiddleware<ErrorHandlingMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthChecks("/health");

        // Ensure database is created and seed default user
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ThAmCoContext>();
            context.Database.Migrate();

            if (!context.Staff.Any(s => s.Email == "staff@thamco.com"))
            {
                context.Staff.Add(new ThAmCo.WebApi.Models.Staff
                {
                    Name = "Staff User",
                    Email = "staff@thamco.com",
                    Password = "staff123",
                    Role = "Staff",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
                context.SaveChanges();
            }
        }

        Console.WriteLine("Application started.");
        app.Run();
    }
}
