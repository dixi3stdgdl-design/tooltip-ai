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
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "tooltipai-backend",
    version = "1.0.0",
    timestamp = DateTime.UtcNow
}));

app.MapGet("/", () => Results.Ok(new
{
    name = "TooltipAI Backend",
    version = "1.0.0",
    endpoints = new[]
    {
        "GET  /health",
        "POST /api/license/validate",
        "GET  /api/context/{key}",
        "POST /api/context",
        "GET  /api/context/stats",
        "GET  /api/plugins",
        "GET  /api/plugins/{id}",
        "POST /api/plugins"
    }
}));

app.Run();
