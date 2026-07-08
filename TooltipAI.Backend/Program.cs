using Microsoft.Azure.Cosmos;
using TooltipAI.Backend.Middleware;
using TooltipAI.Backend.Services;
using TooltipAI.Core.AI;
using TooltipAI.Core.Services;
using TooltipAI.Core.Translate;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Cosmos DB (optional - falls back to in-memory if not configured)
builder.Services.AddSingleton(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("CosmosDb");
    if (string.IsNullOrEmpty(connectionString))
    {
        return (CosmosClient?)null;
    }
    try
    {
        return new CosmosClient(connectionString, new CosmosClientOptions
        {
            ConnectionMode = ConnectionMode.Gateway
        });
    }
    catch (Exception ex)
    {
        var logger = sp.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Failed to connect to Cosmos DB, using in-memory storage");
        return (CosmosClient?)null;
    }
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TooltipAI Backend", Version = "v1" });
});

// Core services
builder.Services.AddSingleton<AuthService>();
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
            ?? new[] {
                "https://tooltip-ai.com",
                "https://www.tooltip-ai.com",
                "https://salmon-dune-08337790f.7.azurestaticapps.net",
                "https://tooltip-ai.azurewebsites.net"
            };

        policy.WithOrigins(origins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
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

// Health check for Azure Auto-Scale (returns 200 OK as plain text)
app.MapGet("/health", () => Results.Content(
    "{\"status\":\"healthy\",\"service\":\"tooltipai-backend\",\"version\":\"1.0.0\"}",
    "application/json"
));

// Readiness probe for Azure
app.MapGet("/ready", () => Results.Content("{\"status\":\"ready\"}", "application/json"));

app.MapGet("/", () => Results.Content(
    "{\"name\":\"TooltipAI Backend\",\"version\":\"1.0.0\",\"platform\":\"Azure Linux 4\"}",
    "application/json"
));

app.Run();

// Expose Program class for integration tests
public partial class Program { }
