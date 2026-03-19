using System.Net;
using System.Net.Http.Json;
using CustomsExitTracking.ServiceB.Api.Application;
using CustomsExitTracking.ServiceB.Api.Repositories;
using CustomsExitTracking.Shared.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CustomsExitTracking.ServiceB.Tests;

public class ReadEndpointsTests
{
    [Fact]
    public async Task GetPerson_ReturnsOkWhenPersonExists()
    {
        var person = new PersonDto(Guid.NewGuid(), "MY9001010001", "Ahmad Firdaus bin Rahman", new DateOnly(1990, 1, 1), "MYS", "M");
        await using var factory = CreateFactory(person, []);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/persons/MY9001010001");
        var payload = await response.Content.ReadFromJsonAsync<PersonDto>();

        response.EnsureSuccessStatusCode();
        Assert.NotNull(payload);
        Assert.Equal(person.NationalId, payload.NationalId);
    }

    [Fact]
    public async Task GetPerson_ReturnsNotFoundWhenMissing()
    {
        await using var factory = CreateFactory(null, []);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/persons/UNKNOWN");
        var payload = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("PERSON_NOT_FOUND", payload.Code);
    }

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
        var exits = new[]
        {
            new ExitRecordDto(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, "MYS", "SGP", "PEN Airport", "MY9001010001", "Business")
        };
        await using var factory = CreateFactory(null, exits);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/persons/MY9001010001/exits?limit=10&offset=0");
        var payload = await response.Content.ReadFromJsonAsync<List<ExitRecordDto>>();

        response.EnsureSuccessStatusCode();
        Assert.NotNull(payload);
        Assert.Single(payload);
    }

    private static WebApplicationFactory<Program> CreateFactory(PersonDto? person, IReadOnlyList<ExitRecordDto> exits) =>
        new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IPersonReadRepository>();
                    services.RemoveAll<IExitRecordReadRepository>();
                    services.RemoveAll<PersonReadService>();
                    services.RemoveAll<ExitHistoryReadService>();

                    services.AddSingleton<IPersonReadRepository>(new StubPersonReadRepository(person));
                    services.AddSingleton<IExitRecordReadRepository>(new StubExitRecordReadRepository(exits));
                    services.AddScoped<PersonReadService>();
                    services.AddScoped<ExitHistoryReadService>();
                });
            });

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
}
