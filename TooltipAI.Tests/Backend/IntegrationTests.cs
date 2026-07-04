using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TooltipAI.Backend.Models;
using Xunit;

namespace TooltipAI.Tests.Backend;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        Environment.SetEnvironmentVariable("License__HmacKey", "integration-test-key-2024");
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<HealthResponse>();
        body!.Status.Should().Be("healthy");
        body.Service.Should().Be("tooltipai-backend");
    }

    [Fact]
    public async Task Root_ReturnsEndpointsList()
    {
        var response = await _client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("/health");
        body.Should().Contain("/api/license/validate");
        body.Should().Contain("/api/context");
        body.Should().Contain("/api/plugins");
    }

    [Fact]
    public async Task LicenseValidate_EmptyKey_ReturnsInvalid()
    {
        var response = await _client.PostAsJsonAsync("/api/license/validate", new LicenseValidateRequest
        {
            LicenseKey = "",
            MachineId = "test",
            AppVersion = "1.0.0"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<LicenseValidateResponse>();
        body!.Valid.Should().BeFalse();
    }

    [Fact]
    public async Task ContextGet_NonExisting_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/context/nonexistent");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ContextSet_ThenGet_ReturnsEntry()
    {
        var setResponse = await _client.PostAsJsonAsync("/api/context", new ContextCacheRequest
        {
            Key = "integration-key",
            ElementName = "Button",
            ElementType = "Button",
            ApplicationName = "TestApp",
            Context = "Integration test context",
            Tags = new[] { "test" },
            Confidence = 95,
            TtlSeconds = 60
        });

        setResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await _client.GetAsync("/api/context/integration-key");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var entry = await getResponse.Content.ReadFromJsonAsync<ContextEntry>();
        entry!.Key.Should().Be("integration-key");
        entry.Context.Should().Be("Integration test context");
    }

    [Fact]
    public async Task ContextStats_ReturnsStats()
    {
        var response = await _client.GetAsync("/api/context/stats");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var stats = await response.Content.ReadFromJsonAsync<ContextCacheStats>();
        stats.Should().NotBeNull();
    }

    [Fact]
    public async Task PluginsGetAll_ReturnsList()
    {
        var response = await _client.GetAsync("/api/plugins");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("tooltipai-context-basic");
    }

    [Fact]
    public async Task PluginsGetById_Existing_ReturnsPlugin()
    {
        var response = await _client.GetAsync("/api/plugins/tooltipai-context-basic");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var plugin = await response.Content.ReadFromJsonAsync<PluginInfo>();
        plugin!.Name.Should().Be("Basic Context Pack");
    }

    [Fact]
    public async Task PluginsGetById_NonExisting_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/plugins/nonexistent");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PluginsStats_ReturnsStats()
    {
        var response = await _client.GetAsync("/api/plugins/stats");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var stats = await response.Content.ReadFromJsonAsync<PluginRegistryStats>();
        stats!.TotalPlugins.Should().BeGreaterThanOrEqualTo(2);
    }

    private sealed class HealthResponse
    {
        public string Status { get; init; } = string.Empty;
        public string Service { get; init; } = string.Empty;
        public string Version { get; init; } = string.Empty;
    }
}
