using CustomsExitTracking.ServiceB.Api.Application;
using CustomsExitTracking.ServiceB.Api.Contracts;
using CustomsExitTracking.ServiceB.Api.Repositories;
using CustomsExitTracking.Shared.Contracts;

namespace CustomsExitTracking.ServiceB.Tests;

public class CreateExitRecordServiceTests
{
    [Fact]
    public async Task CreateAsync_ReturnsNull_WhenPersonDoesNotExist()
    {
        var service = new ExitRecordService(
            new StubPersonReadRepository(null),
            new StubExitRecordRepository());

        var result = await service.CreateAsync(
            "UNKNOWN",
            new ExitRecordCreateRequest(DateTimeOffset.UtcNow, "MYS", "SGP", "PEN Airport", null, "Business"),
            CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_CreatesExitRecord_WhenPersonExists()
    {
        var person = new PersonDto(Guid.NewGuid(), "MY9001010001", "Ahmad Firdaus bin Rahman", new DateOnly(1990, 1, 1), "MYS", "M");
        var service = new ExitRecordService(
            new StubPersonReadRepository(person),
            new StubExitRecordRepository());

        var result = await service.CreateAsync(
            person.NationalId,
            new ExitRecordCreateRequest(DateTimeOffset.UtcNow, "MYS", "SGP", "PEN Airport", "MY9001010001", "Business"),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(person.PersonId, result.PersonId);
        Assert.Equal("SGP", result.ToCountryCode);
    }

    private sealed class StubPersonReadRepository(PersonDto? person) : IPersonReadRepository
    {
        public Task<PersonDto?> GetByNationalIdAsync(string nationalId, CancellationToken cancellationToken) =>
            Task.FromResult(person);
    }

    private sealed class StubExitRecordRepository : IExitRecordRepository
    {
        public Task<IReadOnlyList<ExitRecordDto>> GetByNationalIdAsync(
            string nationalId,
            ExitRecordQueryRequest request,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

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
