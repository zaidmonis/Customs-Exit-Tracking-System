using CustomsExitTracking.ServiceA.Api.Application;
using CustomsExitTracking.ServiceA.Api.Repositories;
using CustomsExitTracking.Shared.Contracts;

namespace CustomsExitTracking.ServiceA.Tests;

public class ReadServicesTests
{
    [Fact]
    public async Task PersonReadService_ReturnsRepositoryResult()
    {
        var expected = new PersonDto(Guid.NewGuid(), "MY9001010001", "Ahmad Firdaus bin Rahman", new DateOnly(1990, 1, 1), "MYS", "M");
        var service = new PersonReadService(new StubPersonReadRepository(expected));

        var result = await service.GetByNationalIdAsync("MY9001010001", CancellationToken.None);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task ExitRecordReadService_ReturnsRepositoryResults()
    {
        var expected = new[]
        {
            new ExitRecordDto(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, "MYS", "SGP", "PEN Airport", "MY9001010001", "Business")
        };
        var service = new ExitRecordReadService(new StubExitRecordReadRepository(expected));

        var result = await service.GetByNationalIdAsync("MY9001010001", new ExitRecordQueryRequest(null, null, null), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(expected[0], result[0]);
    }

    private sealed class StubPersonReadRepository(PersonDto? person) : IPersonReadRepository
    {
        public Task<PersonDto?> GetByNationalIdAsync(string nationalId, CancellationToken cancellationToken) =>
            Task.FromResult(person);
    }

    private sealed class StubExitRecordReadRepository(IReadOnlyList<ExitRecordDto> results) : IExitRecordReadRepository
    {
        public Task<IReadOnlyList<ExitRecordDto>> GetByNationalIdAsync(
            string nationalId,
            ExitRecordQueryRequest request,
            CancellationToken cancellationToken) =>
            Task.FromResult(results);
    }
}
