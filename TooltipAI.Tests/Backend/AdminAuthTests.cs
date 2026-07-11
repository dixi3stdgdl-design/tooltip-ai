using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace TooltipAI.Tests.Backend;

public class AdminAuthTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string AdminKey = "unit-test-admin-key-2024";
    private readonly WebApplicationFactory<Program> _factory;

    public AdminAuthTests(WebApplicationFactory<Program> factory)
    {
        Environment.SetEnvironmentVariable("License__HmacKey", "admin-auth-test-key-2024");
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Admin:ApiKey"] = AdminKey
                });
            });
        });
    }

    [Fact]
    public async Task AdminEndpoint_WithoutKey_IsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/admin/health");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminEndpoint_WithWrongKey_IsUnauthorized()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Admin-Key", "wrong-key");

        var response = await client.GetAsync("/api/admin/health");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminEndpoint_WithValidKey_Succeeds()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Admin-Key", AdminKey);

        var response = await client.GetAsync("/api/admin/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task LicenseGenerate_WithoutKey_IsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/license/generate", new
        {
            LicenseId = "LIC-TEST",
            Tier = "enterprise"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LicenseValidate_RemainsPublic()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/license/validate", new
        {
            LicenseKey = "some-key",
            MachineId = "m",
            AppVersion = "1.0.0"
        });

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }
}
