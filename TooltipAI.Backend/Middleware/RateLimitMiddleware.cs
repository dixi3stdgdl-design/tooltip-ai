using System.Collections.Concurrent;

namespace TooltipAI.Backend.Middleware;

public sealed class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly int _maxRequests;
    private readonly int _windowSeconds;
    private readonly ConcurrentDictionary<string, ClientRateInfo> _clients = new();

    public RateLimitMiddleware(RequestDelegate next, int maxRequests = 100, int windowSeconds = 60)
    {
        _next = next;
        _maxRequests = maxRequests;
        _windowSeconds = windowSeconds;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var now = DateTime.UtcNow;

        var client = _clients.AddOrUpdate(clientIp,
            _ => new ClientRateInfo { WindowStart = now, Count = 1 },
            (_, existing) =>
            {
                if ((now - existing.WindowStart).TotalSeconds > _windowSeconds)
                {
                    existing.WindowStart = now;
                    existing.Count = 1;
                }
                else
                {
                    existing.Count++;
                }
                return existing;
            });

        if (client.Count > _maxRequests)
        {
            context.Response.StatusCode = 429;
            context.Response.Headers["Retry-After"] = _windowSeconds.ToString();
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Rate limit exceeded",
                retryAfter = _windowSeconds
            });
            return;
        }

        context.Response.Headers["X-RateLimit-Limit"] = _maxRequests.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = Math.Max(0, _maxRequests - client.Count).ToString();

        await _next(context);
    }

    private sealed class ClientRateInfo
    {
        public DateTime WindowStart { get; set; }
        public int Count { get; set; }
    }
}
