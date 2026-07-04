using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using TooltipAI.Backend.Middleware;
using Xunit;

namespace TooltipAI.Tests.Backend;

public class RateLimitMiddlewareTests
{
    [Fact]
    public async Task UnderLimit_ReturnsNext()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");

        var nextCalled = false;
        var middleware = new RateLimitMiddleware(
            _ => { nextCalled = true; return Task.CompletedTask; },
            maxRequests: 5,
            windowSeconds: 60);

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task OverLimit_Returns429()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.1");
        context.Response.Body = new MemoryStream();

        var middleware = new RateLimitMiddleware(
            _ => Task.CompletedTask,
            maxRequests: 2,
            windowSeconds: 60);

        for (int i = 0; i < 3; i++)
        {
            var ctx = new DefaultHttpContext();
            ctx.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.1");
            ctx.Response.Body = new MemoryStream();
            await middleware.InvokeAsync(ctx);
        }

        var blockedContext = new DefaultHttpContext();
        blockedContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.1");
        blockedContext.Response.Body = new MemoryStream();
        await middleware.InvokeAsync(blockedContext);

        blockedContext.Response.StatusCode.Should().Be(429);
    }

    [Fact]
    public async Task DifferentIPs_HaveSeparateLimits()
    {
        var middleware = new RateLimitMiddleware(
            _ => Task.CompletedTask,
            maxRequests: 1,
            windowSeconds: 60);

        var ctx1 = new DefaultHttpContext();
        ctx1.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("10.0.0.1");
        ctx1.Response.Body = new MemoryStream();
        await middleware.InvokeAsync(ctx1);
        ctx1.Response.StatusCode.Should().Be(200);

        var ctx2 = new DefaultHttpContext();
        ctx2.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("10.0.0.2");
        ctx2.Response.Body = new MemoryStream();
        await middleware.InvokeAsync(ctx2);
        ctx2.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Sets_RateLimitHeaders()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("10.0.0.5");
        context.Response.Body = new MemoryStream();

        var middleware = new RateLimitMiddleware(
            _ => Task.CompletedTask,
            maxRequests: 100,
            windowSeconds: 60);

        await middleware.InvokeAsync(context);

        context.Response.Headers.ContainsKey("X-RateLimit-Limit").Should().BeTrue();
        context.Response.Headers.ContainsKey("X-RateLimit-Remaining").Should().BeTrue();
    }
}

public class SecurityHeadersMiddlewareTests
{
    [Fact]
    public async Task Sets_SecurityHeaders()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new SecurityHeadersMiddleware(
            _ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        context.Response.Headers["X-Content-Type-Options"].ToString().Should().Be("nosniff");
        context.Response.Headers["X-Frame-Options"].ToString().Should().Be("DENY");
        context.Response.Headers["X-XSS-Protection"].ToString().Should().Contain("1; mode=block");
        context.Response.Headers["Referrer-Policy"].ToString().Should().Be("strict-origin-when-cross-origin");
        context.Response.Headers["Strict-Transport-Security"].ToString().Should().Contain("max-age=31536000");
    }

    [Fact]
    public async Task Sets_PermissionsPolicy()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new SecurityHeadersMiddleware(
            _ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        context.Response.Headers["Permissions-Policy"].ToString().Should().Contain("camera=()");
        context.Response.Headers["Permissions-Policy"].ToString().Should().Contain("microphone=()");
    }
}

public class RequestLoggingMiddlewareTests
{
    [Fact]
    public async Task SetsRequestId_Header()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var logger = Mock.Of<ILogger<RequestLoggingMiddleware>>();
        var middleware = new RequestLoggingMiddleware(
            _ => Task.CompletedTask, logger);

        await middleware.InvokeAsync(context);

        context.Response.Headers.ContainsKey("X-Request-Id").Should().BeTrue();
        context.Response.Headers["X-Request-Id"].ToString().Length.Should().Be(8);
    }

    [Fact]
    public async Task CallsNext_Middleware()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var nextCalled = false;
        var logger = Mock.Of<ILogger<RequestLoggingMiddleware>>();
        var middleware = new RequestLoggingMiddleware(
            _ => { nextCalled = true; return Task.CompletedTask; }, logger);

        await middleware.InvokeAsync(context);

        nextCalled.Should().BeTrue();
    }
}
