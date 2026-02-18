using System.Diagnostics;

namespace FacilitationAssistant.Web.Middleware;

/// <summary>
/// Middleware that logs all unhandled exceptions with full context for Application Insights
/// </summary>
public class ExceptionLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionLoggingMiddleware> _logger;

    public ExceptionLoggingMiddleware(RequestDelegate next, ILogger<ExceptionLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var requestId = Activity.Current?.Id ?? context.TraceIdentifier;
            
            _logger.LogError(ex, 
                "Unhandled exception occurred. RequestId: {RequestId}, Path: {Path}, Method: {Method}, User: {User}",
                requestId,
                context.Request.Path,
                context.Request.Method,
                context.User?.Identity?.Name ?? "Anonymous");
            
            throw;
        }
    }
}
