using System.Net.Http.Json;
using CustomsExitTracking.ServiceB.Api.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CustomsExitTracking.ServiceB.Tests;

public sealed class UnitTest1 : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;

    public UnitTest1(WebApplicationFactory<Program> factory)
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
