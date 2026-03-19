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

public class PersonEndpointsTests
{
    [Fact]
    public async Task GetPerson_ReturnsOkWhenPersonExists()
    {
        var person = TestData.CreatePerson();
        await using var factory = CreateFactory(person);
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
        await using var factory = CreateFactory(null);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/persons/UNKNOWN");
        var payload = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal("PERSON_NOT_FOUND", payload.Code);
    }

    private static WebApplicationFactory<Program> CreateFactory(PersonDto? person) =>
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
                    services.AddSingleton<IExitRecordRepository>(new StubExitRecordRepository([], true));
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
            throw new NotSupportedException();

        public Task<ExitRecordDto?> UpdateAsync(
            Guid personId,
            ExitRecordUpdateRequest request,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<bool> DeleteAsync(Guid personId, Guid exitId, CancellationToken cancellationToken) =>
            Task.FromResult(deleteResult);
    }
}
