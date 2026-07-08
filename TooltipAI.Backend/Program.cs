using System.Text.Json.Serialization;
using TooltipAI.Backend.Middleware;
using TooltipAI.Backend.Services;
using TooltipAI.Core.AI;
using TooltipAI.Core.Services;
using TooltipAI.Core.Translate;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TooltipAI Backend", Version = "v1" });
});

// Core services
builder.Services.AddSingleton<TooltipAI.Backend.Services.LicenseService>();
builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddSingleton<PaymentService>();
builder.Services.AddSingleton<ContextCacheService>();
builder.Services.AddSingleton<PluginRegistryService>();
builder.Services.AddSingleton<PIIFilter>(PIIFilter.Instance);
builder.Services.AddSingleton<TelemetryAggregator>();
builder.Services.AddSingleton<EnrichmentEngine>();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<LLMProvider>();

// Shared HttpClient for manually-constructed providers (CloudLLMProvider)
builder.Services.AddSingleton(new HttpClient { Timeout = TimeSpan.FromSeconds(30) });

// Hybrid AI System (Gemini Nano + Cloud LLM + Router)
builder.Services.AddSingleton<GeminiNanoProvider>(sp =>
    new GeminiNanoProvider(sp.GetRequiredService<ILogger<GeminiNanoProvider>>()));
builder.Services.AddSingleton<CloudLLMProvider>(sp =>
    new CloudLLMProvider(
        sp.GetRequiredService<HttpClient>(),
        sp.GetRequiredService<ILogger<CloudLLMProvider>>(),
        builder.Configuration["CloudLLM:ApiKey"],
        builder.Configuration["CloudLLM:Endpoint"],
        builder.Configuration["CloudLLM:Model"]));
builder.Services.AddSingleton<AIRouter>();

// Translate Module (Gemini Nano powered - zero cost)
builder.Services.AddSingleton<LanguageDetector>();
builder.Services.AddSingleton<Translator>();
builder.Services.AddSingleton<ConversationMode>();

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

// Only enable HTTPS redirection if not running in a container
if (!Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER")?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? true)
{
    app.UseHttpsRedirection();
}
app.UseCors();
app.MapControllers();

// Health check for Azure Auto-Scale (returns 200 OK)
app.MapGet("/health", () => Results.Ok(new HealthResponse(
    "healthy", "tooltipai-backend", "1.0.0",
    Environment.OSVersion.ToString(),
    System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
    DateTime.UtcNow,
    Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID") ?? "local"
)));

// Readiness probe for Azure
app.MapGet("/ready", () => Results.Ok(new StatusResponse("ready")));

app.MapGet("/", () => Results.Ok(new RootResponse(
    "TooltipAI Backend", "1.0.0", "Azure Linux 4",
    new[] { "GET  /health", "GET  /ready", "POST /api/license/validate",
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

// Concrete types for JSON serialization (anonymous types fail with .NET 8 source gen)
public record HealthResponse(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("service")] string Service,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("os")] string Os,
    [property: JsonPropertyName("runtime")] string Runtime,
    [property: JsonPropertyName("timestamp")] DateTime Timestamp,
    [property: JsonPropertyName("instanceId")] string InstanceId
);

public record StatusResponse(
    [property: JsonPropertyName("status")] string Status
);

public record RootResponse(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("platform")] string Platform,
    [property: JsonPropertyName("endpoints")] string[] Endpoints
);
