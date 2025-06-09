using System.Diagnostics;

namespace ThAmCo.WebApi.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = Guid.NewGuid().ToString();
        context.Items["RequestId"] = requestId;

        var stopwatch = Stopwatch.StartNew();
        
        // Log request
        _logger.LogInformation("Request {RequestId}: {Method} {Path} started at {Timestamp}",
            requestId,
            context.Request.Method,
            context.Request.Path,
            DateTime.UtcNow);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            
            // Log response
            _logger.LogInformation("Request {RequestId}: {Method} {Path} completed with {StatusCode} in {ElapsedMs}ms",
                requestId,
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
