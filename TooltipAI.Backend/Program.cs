using TooltipAI.Backend.Middleware;
using TooltipAI.Backend.Services;
using TooltipAI.Core.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TooltipAI Backend", Version = "v1" });
});

builder.Services.AddSingleton<TooltipAI.Backend.Services.LicenseService>();
builder.Services.AddSingleton<ContextCacheService>();
builder.Services.AddSingleton<PluginRegistryService>();
builder.Services.AddSingleton<PIIFilter>();
builder.Services.AddSingleton<TelemetryAggregator>();
builder.Services.AddSingleton<EnrichmentEngine>();
builder.Services.AddHttpClient<LLMProvider>();
builder.Services.AddMemoryCache();

// Redis cache for distributed scenarios (optional)
// Uncomment when adding Redis NuGet package: Microsoft.Extensions.Caching.StackExchangeRedis
// var redisConnection = builder.Configuration.GetConnectionString("Redis");
// if (!string.IsNullOrEmpty(redisConnection))
// {
//     builder.Services.AddStackExchangeRedisCache(options =>
//     {
//         options.Configuration = redisConnection;
//     });
// }

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
            ?? new[] { "https://tooltip-ai.com", "https://www.tooltip-ai.com" };

        policy.WithOrigins(origins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure Kestrel for high throughput
builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
    options.Limits.MaxConcurrentConnections = 500;
    options.Limits.MaxConcurrentUpgradedConnections = 500;
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

// Rate limit: 1000 requests per 60 seconds per IP
// This handles 500 concurrent users easily
app.UseMiddleware<RateLimitMiddleware>(1000, 60);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.MapControllers();

// Health check for Azure Auto-Scale (returns 200 OK)
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "tooltipai-backend",
    version = "1.0.0",
    os = Environment.OSVersion.ToString(),
    runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
    timestamp = DateTime.UtcNow,
    instanceId = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID") ?? "local"
}));

// Readiness probe for Azure
app.MapGet("/ready", () => Results.Ok(new { status = "ready" }));

app.MapGet("/", () => Results.Ok(new
{
    name = "TooltipAI Backend",
    version = "1.0.0",
    platform = "Azure Linux 4",
    endpoints = new[]
    {
        "GET  /health",
        "GET  /ready",
        "POST /api/license/validate",
        "POST /api/license/generate",
        "GET  /api/context/{key}",
        "POST /api/context",
        "GET  /api/context/stats",
        "GET  /api/plugins",
        "GET  /api/plugins/{id}",
        "POST /api/plugins",
        "GET  /api/plugins/stats",
        "POST /api/admin/provision",
        "GET  /api/admin/users",
        "PUT  /api/admin/policies",
        "GET  /api/admin/metrics",
        "POST /api/admin/rollout",
        "POST /api/enrich",
        "GET  /api/enrich/health",
        "POST /api/telemetry",
        "GET  /api/telemetry/metrics",
        "GET  /api/telemetry/health"
    }
}));

app.Run();

// Expose Program class for integration tests
public partial class Program { }
