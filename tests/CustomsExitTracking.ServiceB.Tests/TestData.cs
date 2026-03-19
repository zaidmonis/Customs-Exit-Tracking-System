using CustomsExitTracking.Shared.Contracts;

namespace CustomsExitTracking.ServiceB.Tests;

internal static class TestData
{
    public static PersonDto CreatePerson() =>
        new(Guid.NewGuid(), "MY9001010001", "Ahmad Firdaus bin Rahman", new DateOnly(1990, 1, 1), "MYS", "M");

    public static ExitRecordDto CreateExitRecord() =>
        new(Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, "MYS", "SGP", "PEN Airport", "MY9001010001", "Business");
}
