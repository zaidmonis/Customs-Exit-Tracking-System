using CustomsExitTracking.ServiceA.Api.Application;
using CustomsExitTracking.ServiceA.Api.Contracts;
using CustomsExitTracking.ServiceA.Api.Integrations;
using CustomsExitTracking.ServiceA.Api.Repositories;
using CustomsExitTracking.ServiceA.Api.Settings;
using CustomsExitTracking.Shared.Contracts;
using Microsoft.Extensions.Options;

namespace CustomsExitTracking.ServiceA.Tests;

public class VerifyAndInsertServiceTests
{
    [Fact]
    public async Task VerifyAndInsertAsync_RejectsWhenPersonIsMissing()
    {
        var service = CreateService(null, [], 3);

        var result = await service.VerifyAndInsertAsync(
            "UNKNOWN",
            CreateRequest(),
            CancellationToken.None);

        Assert.Equal(VerifyDecision.RejectPersonNotFound, result.Decision);
        Assert.False(result.InsertPerformed);
        Assert.Equal("PERSON_NOT_FOUND", result.ReasonCode);
    }

    [Fact]
    public async Task VerifyAndInsertAsync_FlagsWhenRecentExitCountExceedsThreshold()
    {
        var person = CreatePerson();
        var exits = new[]
        {
            CreateExitRecord(),
            CreateExitRecord(),
            CreateExitRecord(),
            CreateExitRecord()
        };
        var service = CreateService(person, exits, 3);

        var result = await service.VerifyAndInsertAsync(
            person.NationalId,
            CreateRequest(),
            CancellationToken.None);

        Assert.Equal(VerifyDecision.FlagFrequentTravel, result.Decision);
        Assert.False(result.InsertPerformed);
        Assert.Equal(4, result.RecentExitCount);
        Assert.Equal("FREQUENT_TRAVEL", result.ReasonCode);
    }

    [Fact]
    public async Task VerifyAndInsertAsync_PassesAndInsertsWhenWithinThreshold()
    {
        var person = CreatePerson();
        var exits = new[] { CreateExitRecord() };
        var service = CreateService(person, exits, 3);

        var result = await service.VerifyAndInsertAsync(
            person.NationalId,
            CreateRequest(),
            CancellationToken.None);

        Assert.Equal(VerifyDecision.Pass, result.Decision);
        Assert.True(result.InsertPerformed);
        Assert.Equal(1, result.RecentExitCount);
    }

    private static ExitVerificationService CreateService(
        PersonDto? person,
        IReadOnlyList<ExitRecordDto> exits,
        int threshold) =>
        new(
            new StubPersonReadRepository(person),
            new StubExitRecordReadRepository(exits),
            new StubServiceBClient(),
            Options.Create(new ScreeningRulesOptions { FrequentTravelThreshold = threshold }));

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
