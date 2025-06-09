using Microsoft.EntityFrameworkCore;
using ThAmCo.WebApi.Models;
using BCrypt.Net;

namespace ThAmCo.WebApi.Data;

public class ThAmCoContext : DbContext
{
    public ThAmCoContext(DbContextOptions<ThAmCoContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<ProductSupplier> ProductSuppliers { get; set; }
    public DbSet<Staff> Staff { get; set; }
    public DbSet<CustomerAuditLog> CustomerAuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Product entity
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.BasePrice).HasColumnType("decimal(10,2)");
            entity.Property(e => e.CurrentPrice).HasColumnType("decimal(10,2)");
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Category);
        });

        // Configure Customer entity
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.AccountFunds).HasColumnType("decimal(10,2)");
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Configure Order entity
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10,2)");
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.Status);
            
            entity.HasOne(e => e.Customer)
                  .WithMany(c => c.Orders)
                  .HasForeignKey(e => e.CustomerId);
        });

        // Configure OrderItem entity
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10,2)");
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(10,2)");
            
            entity.HasOne(e => e.Order)
                  .WithMany(o => o.OrderItems)
                  .HasForeignKey(e => e.OrderId);
                  
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId);
        });

        // Configure ProductSupplier entity
        modelBuilder.Entity<ProductSupplier>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SupplierPrice).HasColumnType("decimal(10,2)");
            
            entity.HasOne(e => e.Product)
                  .WithMany(p => p.ProductSuppliers)
                  .HasForeignKey(e => e.ProductId);
                  
            entity.HasOne(e => e.Supplier)
                  .WithMany()
                  .HasForeignKey(e => e.SupplierId);
        });

        // Configure Supplier entity
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ApiEndpoint).IsRequired().HasMaxLength(500);
        });

        // Configure Staff entity
        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Seed data
        modelBuilder.Entity<Supplier>().HasData(
            new Supplier { Id = 1, Name = "Electronics Supplier", ApiEndpoint = "http://localhost:5001/api/inventory" },
            new Supplier { Id = 2, Name = "Books Supplier", ApiEndpoint = "http://localhost:5002/api/inventory" }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Laptop", Description = "Powerful laptop for all your needs.", BasePrice = 1200.00m, CurrentPrice = 1320.00m, StockQuantity = 10, Category = "Electronics", ImageUrl = "" },
            new Product { Id = 2, Name = "Smartphone", Description = "Latest model smartphone with great camera.", BasePrice = 800.00m, CurrentPrice = 880.00m, StockQuantity = 25, Category = "Electronics", ImageUrl = "" },
            new Product { Id = 3, Name = "The Great Gatsby", Description = "Classic novel by F. Scott Fitzgerald.", BasePrice = 10.00m, CurrentPrice = 11.00m, StockQuantity = 50, Category = "Books", ImageUrl = "" },
            new Product { Id = 4, Name = "Noise-Cancelling Headphones", Description = "Immersive audio experience.", BasePrice = 150.00m, CurrentPrice = 165.00m, StockQuantity = 15, Category = "Electronics", ImageUrl = "" },
            new Product { Id = 5, Name = "The Hitchhiker's Guide to the Galaxy", Description = "A comedic science fiction series.", BasePrice = 8.00m, CurrentPrice = 8.80m, StockQuantity = 40, Category = "Books", ImageUrl = "" }
        );

        modelBuilder.Entity<ProductSupplier>().HasData(
            new ProductSupplier { Id = 1, ProductId = 1, SupplierId = 1, SupplierPrice = 1200.00m },
            new ProductSupplier { Id = 2, ProductId = 2, SupplierId = 1, SupplierPrice = 800.00m },
            new ProductSupplier { Id = 3, ProductId = 3, SupplierId = 2, SupplierPrice = 10.00m },
            new ProductSupplier { Id = 4, ProductId = 4, SupplierId = 1, SupplierPrice = 150.00m },
            new ProductSupplier { Id = 5, ProductId = 5, SupplierId = 2, SupplierPrice = 8.00m }
        );

        // Seed default Staff user
        var defaultStaffPasswordHash = BCrypt.Net.BCrypt.HashPassword("StaffPass123!");
        modelBuilder.Entity<Staff>().HasData(
            new Staff { Id = 1, Name = "Staff Admin", Email = "staff@thamco.com", Password = defaultStaffPasswordHash, Role = "Staff", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );

        // Seed default Customer user
        var defaultCustomerPasswordHash = BCrypt.Net.BCrypt.HashPassword("CustomerPass123!");
        modelBuilder.Entity<Customer>().HasData(
            new Customer { Id = 1, Name = "Test Customer", Email = "customer@example.com", Password = defaultCustomerPasswordHash, AccountFunds = 5000000.00m, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, DeliveryAddress = "123 Customer St, Custville", PhoneNumber = "+1234567890" }
        );
    }
}
