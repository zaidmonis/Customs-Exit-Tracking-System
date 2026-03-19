using System.Net;
using System.Net.Http.Json;
using CustomsExitTracking.ServiceA.Api.Application;
using CustomsExitTracking.ServiceA.Api.Contracts;
using CustomsExitTracking.ServiceA.Api.Integrations;
using CustomsExitTracking.ServiceA.Api.Repositories;
using CustomsExitTracking.ServiceA.Api.Settings;
using CustomsExitTracking.Shared.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CustomsExitTracking.ServiceA.Tests;

public class VerifyAndInsertEndpointsTests
{
    [Fact]
    public async Task VerifyAndInsert_ReturnsReject_WhenPersonIsMissing()
    {
        await using var factory = CreateFactory(null, []);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/persons/UNKNOWN", CreateRequest());
        var payload = await response.Content.ReadFromJsonAsync<VerifyAndInsertExitResponse>();

        response.EnsureSuccessStatusCode();
        Assert.NotNull(payload);
        Assert.Equal(VerifyDecision.RejectPersonNotFound, payload.Decision);
        Assert.False(payload.InsertPerformed);
    }

    [Fact]
    public async Task VerifyAndInsert_ReturnsFlag_WhenThresholdIsExceeded()
    {
        var person = CreatePerson();
        var exits = new[] { CreateExitRecord(), CreateExitRecord(), CreateExitRecord(), CreateExitRecord() };
        await using var factory = CreateFactory(person, exits);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync($"/api/persons/{person.NationalId}", CreateRequest());
        var payload = await response.Content.ReadFromJsonAsync<VerifyAndInsertExitResponse>();

        response.EnsureSuccessStatusCode();
        Assert.NotNull(payload);
        Assert.Equal(VerifyDecision.FlagFrequentTravel, payload.Decision);
        Assert.False(payload.InsertPerformed);
    }

    [Fact]
    public async Task VerifyAndInsert_ReturnsPass_WhenInsertSucceeds()
    {
        var person = CreatePerson();
        var exits = new[] { CreateExitRecord() };
        await using var factory = CreateFactory(person, exits);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync($"/api/persons/{person.NationalId}", CreateRequest());
        var payload = await response.Content.ReadFromJsonAsync<VerifyAndInsertExitResponse>();

        response.EnsureSuccessStatusCode();
        Assert.NotNull(payload);
        Assert.Equal(VerifyDecision.Pass, payload.Decision);
        Assert.True(payload.InsertPerformed);
    }

    [Fact]
    public async Task VerifyAndInsert_ReturnsBadRequest_ForInvalidPayload()
    {
        await using var factory = CreateFactory(null, []);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/persons/MY9001010001",
            new VerifyAndInsertExitRequest(DateTimeOffset.UtcNow, "MY", "sg", "", null, "Business"));
        var payload = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("VALIDATION_ERROR", payload.Code);
    }

    private static WebApplicationFactory<Program> CreateFactory(PersonDto? person, IReadOnlyList<ExitRecordDto> exits) =>
        new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IPersonReadRepository>();
                    services.RemoveAll<IExitRecordReadRepository>();
                    services.RemoveAll<IServiceBClient>();
                    services.RemoveAll<PersonReadService>();
                    services.RemoveAll<ExitRecordReadService>();
                    services.RemoveAll<ExitVerificationService>();

                    services.Configure<ScreeningRulesOptions>(options => options.FrequentTravelThreshold = 3);
                    services.AddSingleton<IPersonReadRepository>(new StubPersonReadRepository(person));
                    services.AddSingleton<IExitRecordReadRepository>(new StubExitRecordReadRepository(exits));
                    services.AddSingleton<IServiceBClient>(new StubServiceBClient());
                    services.AddScoped<PersonReadService>();
                    services.AddScoped<ExitRecordReadService>();
                    services.AddScoped<ExitVerificationService>();
                });
            });

    private static PersonDto CreatePerson() =>
        new(Guid.NewGuid(), "MY9001010001", "Ahmad Firdaus bin Rahman", new DateOnly(1990, 1, 1), "MYS", "M");

    private static ExitRecordDto CreateExitRecord() =>
        new(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, "MYS", "SGP", "PEN Airport", "MY9001010001", "Business");

    private static VerifyAndInsertExitRequest CreateRequest() =>
        new(DateTimeOffset.UtcNow, "MYS", "SGP", "PEN Airport", "MY9001010001", "Business");

    private sealed class StubPersonReadRepository(PersonDto? person) : IPersonReadRepository
    {
        public Task<PersonDto?> GetByNationalIdAsync(string nationalId, CancellationToken cancellationToken) =>
            Task.FromResult(person);
    }

    private sealed class StubExitRecordReadRepository(IReadOnlyList<ExitRecordDto> exits) : IExitRecordReadRepository
    {
        public Task<IReadOnlyList<ExitRecordDto>> GetByNationalIdAsync(
            string nationalId,
            ExitRecordQueryRequest request,
            CancellationToken cancellationToken) =>
            Task.FromResult(exits);
    }

    private sealed class StubServiceBClient : IServiceBClient
    {
        public Task<ExitRecordDto> CreateExitRecordAsync(
            string nationalId,
            VerifyAndInsertExitRequest request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new ExitRecordDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                request.DepartedAt,
                request.FromCountryCode,
                request.ToCountryCode,
                request.PortOfExit,
                request.TravelDocumentNumber,
                request.Purpose));
    }
}
