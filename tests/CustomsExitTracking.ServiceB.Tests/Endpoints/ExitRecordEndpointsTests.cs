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

public class ExitRecordEndpointsTests
{
    [Fact]
    public async Task GetExits_ReturnsBadRequestForInvalidCountryFilter()
    {
        await using var factory = CreateFactory(null, []);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/persons/MY9001010001/exits?toCountry=sg");
        var payload = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("VALIDATION_ERROR", payload.Code);
    }

    [Fact]
    public async Task GetExits_ReturnsOkWithPagedResults()
    {
        var exits = new[] { TestData.CreateExitRecord() };
        await using var factory = CreateFactory(null, exits);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/persons/MY9001010001/exits?limit=10&offset=0");
        var payload = await response.Content.ReadFromJsonAsync<List<ExitRecordDto>>();

        response.EnsureSuccessStatusCode();
        Assert.NotNull(payload);
        Assert.Single(payload);
    }

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
        var person = TestData.CreatePerson();
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

    [Fact]
    public async Task UpdateExitRecord_ReturnsBadRequest_ForRouteBodyMismatch()
    {
        var person = TestData.CreatePerson();
        await using var factory = CreateFactory(person, []);
        using var client = factory.CreateClient();

        var routeExitId = Guid.NewGuid();
        var bodyExitId = Guid.NewGuid();
        var request = new ExitRecordUpdateRequest(bodyExitId, DateTimeOffset.UtcNow, "MYS", "SGP", "PEN Airport", null, "Business");

        var response = await client.PutAsJsonAsync($"/api/persons/{person.NationalId}/exits/{routeExitId}", request);
        var payload = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("VALIDATION_ERROR", payload.Code);
    }

    [Fact]
    public async Task UpdateExitRecord_ReturnsOk_WhenSuccessful()
    {
        var person = TestData.CreatePerson();
        await using var factory = CreateFactory(person, []);
        using var client = factory.CreateClient();

        var exitId = Guid.NewGuid();
        var request = new ExitRecordUpdateRequest(exitId, DateTimeOffset.UtcNow, "MYS", "THA", "KLIA", null, "Leisure");
        var response = await client.PutAsJsonAsync($"/api/persons/{person.NationalId}/exits/{exitId}", request);
        var payload = await response.Content.ReadFromJsonAsync<ExitRecordDto>();

        response.EnsureSuccessStatusCode();
        Assert.NotNull(payload);
        Assert.Equal(exitId, payload.ExitId);
        Assert.Equal("THA", payload.ToCountryCode);
    }

    [Fact]
    public async Task DeleteExitRecord_ReturnsNoContent_WhenSuccessful()
    {
        var person = TestData.CreatePerson();
        await using var factory = CreateFactory(person, []);
        using var client = factory.CreateClient();

        var response = await client.DeleteAsync($"/api/persons/{person.NationalId}/exits/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteExitRecord_ReturnsNotFound_WhenRecordIsMissing()
    {
        var person = TestData.CreatePerson();
        await using var factory = CreateFactory(person, [], deleteResult: false);
        using var client = factory.CreateClient();

        var response = await client.DeleteAsync($"/api/persons/{person.NationalId}/exits/{Guid.NewGuid()}");
        var payload = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("EXIT_RECORD_NOT_FOUND", payload.Code);
    }

    private static WebApplicationFactory<Program> CreateFactory(
        PersonDto? person,
        IReadOnlyList<ExitRecordDto> exits,
        bool deleteResult = true) =>
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
                    services.AddSingleton<IExitRecordRepository>(new StubExitRecordRepository(exits, deleteResult));
                    services.AddScoped<PersonReadService>();
                    services.AddScoped<ExitRecordService>();
                });
            });

    private sealed class StubPersonReadRepository(PersonDto? person) : IPersonReadRepository
    {
        public Task<PersonDto?> GetByNationalIdAsync(string nationalId, CancellationToken cancellationToken) =>
            Task.FromResult(person);
    }

    private sealed class StubExitRecordRepository(IReadOnlyList<ExitRecordDto> exits, bool deleteResult) : IExitRecordRepository
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

        public Task<ExitRecordDto?> UpdateAsync(
            Guid personId,
            ExitRecordUpdateRequest request,
            CancellationToken cancellationToken) =>
            Task.FromResult<ExitRecordDto?>(new ExitRecordDto(
                request.ExitId,
                personId,
                request.DepartedAt,
                request.FromCountryCode,
                request.ToCountryCode,
                request.PortOfExit,
                request.TravelDocumentNumber,
                request.Purpose));

        public Task<bool> DeleteAsync(Guid personId, Guid exitId, CancellationToken cancellationToken) =>
            Task.FromResult(deleteResult);
    }
}
