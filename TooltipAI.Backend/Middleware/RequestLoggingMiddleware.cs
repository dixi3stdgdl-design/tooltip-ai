using System.Diagnostics;

namespace TooltipAI.Backend.Middleware;

public sealed class RequestLoggingMiddleware
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
        var requestId = Guid.NewGuid().ToString("N")[..8];
        var sw = Stopwatch.StartNew();

        context.Response.Headers["X-Request-Id"] = requestId;

        _logger.LogInformation("[{RequestId}] {Method} {Path} started",
            requestId, context.Request.Method, context.Request.Path);

        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();
            _logger.LogInformation("[{RequestId}] {Method} {Path} completed {StatusCode} in {Elapsed}ms",
                requestId, context.Request.Method, context.Request.Path,
                context.Response.StatusCode, sw.ElapsedMilliseconds);
        }
    }
}
