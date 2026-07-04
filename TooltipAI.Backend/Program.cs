using TooltipAI.Backend.Middleware;
using TooltipAI.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TooltipAI Backend", Version = "v1" });
});

builder.Services.AddSingleton<LicenseService>();
builder.Services.AddSingleton<ContextCacheService>();
builder.Services.AddSingleton<PluginRegistryService>();

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

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<RateLimitMiddleware>(100, 60);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "tooltipai-backend",
    version = "1.0.0",
    os = Environment.OSVersion.ToString(),
    runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
    timestamp = DateTime.UtcNow
}));

app.MapGet("/", () => Results.Ok(new
{
    name = "TooltipAI Backend",
    version = "1.0.0",
    platform = "Azure Linux 4",
    endpoints = new[]
    {
        "GET  /health",
        "POST /api/license/validate",
        "POST /api/license/generate",
        "GET  /api/context/{key}",
        "POST /api/context",
        "GET  /api/context/stats",
        "GET  /api/plugins",
        "GET  /api/plugins/{id}",
        "POST /api/plugins",
        "GET  /api/plugins/stats"
    }
}));

app.Run();

// Expose Program class for integration tests
public partial class Program { }
