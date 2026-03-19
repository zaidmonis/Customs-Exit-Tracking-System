using System.Net.Http.Json;
using CustomsExitTracking.Shared.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CustomsExitTracking.ServiceB.Tests;

public sealed class HealthEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;

    public HealthEndpointsTests(WebApplicationFactory<Program> factory)
    {
        this.factory = factory;
    }

    [Theory]
    [InlineData("/health", "healthy")]
    [InlineData("/ready", "ready")]
    public async Task HealthEndpoints_ReturnExpectedPayload(string path, string expectedStatus)
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync(path);
        var payload = await response.Content.ReadFromJsonAsync<HealthStatusResponse>();

        response.EnsureSuccessStatusCode();
        Assert.NotNull(payload);
        Assert.Equal("service-b", payload.Service);
        Assert.Equal(expectedStatus, payload.Status);
    }
}
