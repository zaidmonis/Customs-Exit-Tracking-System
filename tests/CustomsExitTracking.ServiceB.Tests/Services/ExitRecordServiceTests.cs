using CustomsExitTracking.ServiceB.Api.Application;
using CustomsExitTracking.ServiceB.Api.Contracts;
using CustomsExitTracking.ServiceB.Api.Repositories;
using CustomsExitTracking.Shared.Contracts;

namespace CustomsExitTracking.ServiceB.Tests;

public class ExitRecordServiceTests
{
    [Fact]
    public async Task GetByNationalIdAsync_ReturnsRepositoryResults()
    {
        var expected = new[] { TestData.CreateExitRecord() };
        var service = new ExitRecordService(new StubPersonReadRepository(null), new StubExitRecordRepository(expected));

        var result = await service.GetByNationalIdAsync("MY9001010001", new ExitRecordQueryRequest(null, null, null), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(expected[0], result[0]);
    }

    [Fact]
    public async Task CreateAsync_ReturnsNull_WhenPersonDoesNotExist()
    {
        var service = new ExitRecordService(new StubPersonReadRepository(null), new StubExitRecordRepository());

        var result = await service.CreateAsync(
            "UNKNOWN",
            new ExitRecordCreateRequest(DateTimeOffset.UtcNow, "MYS", "SGP", "PEN Airport", null, "Business"),
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_CreatesExitRecord_WhenPersonExists()
    {
        var person = TestData.CreatePerson();
        var service = new ExitRecordService(new StubPersonReadRepository(person), new StubExitRecordRepository());

        var result = await service.CreateAsync(
            person.NationalId,
            new ExitRecordCreateRequest(DateTimeOffset.UtcNow, "MYS", "SGP", "PEN Airport", "MY9001010001", "Business"),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(person.PersonId, result.PersonId);
        Assert.Equal("SGP", result.ToCountryCode);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenPersonDoesNotExist()
    {
        var service = new ExitRecordService(new StubPersonReadRepository(null), new StubExitRecordRepository());

        var result = await service.UpdateAsync(
            "UNKNOWN",
            new ExitRecordUpdateRequest(Guid.NewGuid(), DateTimeOffset.UtcNow, "MYS", "SGP", "PEN Airport", null, "Business"),
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsUpdatedRecord_WhenPersonExists()
    {
        var person = TestData.CreatePerson();
        var exitId = Guid.NewGuid();
        var service = new ExitRecordService(new StubPersonReadRepository(person), new StubExitRecordRepository());

        var result = await service.UpdateAsync(
            person.NationalId,
            new ExitRecordUpdateRequest(exitId, DateTimeOffset.UtcNow, "MYS", "THA", "KLIA", null, "Leisure"),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(exitId, result.ExitId);
        Assert.Equal("THA", result.ToCountryCode);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsNull_WhenPersonDoesNotExist()
    {
        var service = new ExitRecordService(new StubPersonReadRepository(null), new StubExitRecordRepository());

        var result = await service.DeleteAsync("UNKNOWN", Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenDeleteSucceeds()
    {
        var person = TestData.CreatePerson();
        var service = new ExitRecordService(new StubPersonReadRepository(person), new StubExitRecordRepository());

        var result = await service.DeleteAsync(person.NationalId, Guid.NewGuid(), CancellationToken.None);

        Assert.True(result);
    }

    private sealed class StubPersonReadRepository(PersonDto? person) : IPersonReadRepository
    {
        public Task<PersonDto?> GetByNationalIdAsync(string nationalId, CancellationToken cancellationToken) =>
            Task.FromResult(person);
    }

    private sealed class StubExitRecordRepository(IReadOnlyList<ExitRecordDto>? records = null) : IExitRecordRepository
    {
        public Task<IReadOnlyList<ExitRecordDto>> GetByNationalIdAsync(
            string nationalId,
            ExitRecordQueryRequest request,
            CancellationToken cancellationToken) =>
            Task.FromResult(records ?? (IReadOnlyList<ExitRecordDto>)[TestData.CreateExitRecord()]);

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
            Task.FromResult(true);
    }
}
