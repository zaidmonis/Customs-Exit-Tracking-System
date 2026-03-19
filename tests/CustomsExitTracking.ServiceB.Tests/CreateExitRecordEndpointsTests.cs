using System.Net;
using System.Net.Http.Json;
using CustomsExitTracking.ServiceB.Api.Application;
using CustomsExitTracking.ServiceB.Api.Contracts;
using CustomsExitTracking.ServiceB.Api.Repositories;
using CustomsExitTracking.Shared.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CustomsExitTracking.ServiceB.Tests;

public class CreateExitRecordEndpointsTests
{
    [Fact]
    public async Task CreateExitRecord_ReturnsBadRequest_ForInvalidPayload()
    {
        await using var factory = CreateFactory(null, []);
        using var client = factory.CreateClient();

        var request = new ExitRecordCreateRequest(DateTimeOffset.UtcNow, "MY", "sg", "", null, "Business");
        var response = await client.PostAsJsonAsync("/api/persons/MY9001010001/exits", request);
        var payload = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("VALIDATION_ERROR", payload.Code);
    }

    [Fact]
    public async Task CreateExitRecord_ReturnsNotFound_WhenPersonDoesNotExist()
    {
        await using var factory = CreateFactory(null, []);
        using var client = factory.CreateClient();

        var request = new ExitRecordCreateRequest(DateTimeOffset.UtcNow, "MYS", "SGP", "PEN Airport", null, "Business");
        var response = await client.PostAsJsonAsync("/api/persons/UNKNOWN/exits", request);
        var payload = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("PERSON_NOT_FOUND", payload.Code);
    }

    [Fact]
    public async Task CreateExitRecord_ReturnsCreated_WhenSuccessful()
    {
        var person = new PersonDto(Guid.NewGuid(), "MY9001010001", "Ahmad Firdaus bin Rahman", new DateOnly(1990, 1, 1), "MYS", "M");
        await using var factory = CreateFactory(person, []);
        using var client = factory.CreateClient();

        var request = new ExitRecordCreateRequest(DateTimeOffset.UtcNow, "MYS", "SGP", "PEN Airport", "MY9001010001", "Business");
        var response = await client.PostAsJsonAsync($"/api/persons/{person.NationalId}/exits", request);
        var payload = await response.Content.ReadFromJsonAsync<ExitRecordDto>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(person.PersonId, payload.PersonId);
        Assert.Equal("SGP", payload.ToCountryCode);
    }

    private static WebApplicationFactory<Program> CreateFactory(PersonDto? person, IReadOnlyList<ExitRecordDto> exits) =>
        new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IPersonReadRepository>();
                    services.RemoveAll<IExitRecordRepository>();
                    services.RemoveAll<PersonReadService>();
                    services.RemoveAll<ExitRecordService>();

                    services.AddSingleton<IPersonReadRepository>(new StubPersonReadRepository(person));
                    services.AddSingleton<IExitRecordRepository>(new StubExitRecordRepository(exits));
                    services.AddScoped<PersonReadService>();
                    services.AddScoped<ExitRecordService>();
                });
            });

    private sealed class StubPersonReadRepository(PersonDto? person) : IPersonReadRepository
    {
        public Task<PersonDto?> GetByNationalIdAsync(string nationalId, CancellationToken cancellationToken) =>
            Task.FromResult(person);
    }

    private sealed class StubExitRecordRepository(IReadOnlyList<ExitRecordDto> exits) : IExitRecordRepository
    {
        public Task<IReadOnlyList<ExitRecordDto>> GetByNationalIdAsync(
            string nationalId,
            ExitRecordQueryRequest request,
            CancellationToken cancellationToken) =>
            Task.FromResult(exits);

        public Task<ExitRecordDto> CreateAsync(
            Guid personId,
            ExitRecordCreateRequest request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new ExitRecordDto(
                Guid.NewGuid(),
                personId,
                request.DepartedAt,
                request.FromCountryCode,
                request.ToCountryCode,
                request.PortOfExit,
                request.TravelDocumentNumber,
                request.Purpose));
    }
}
