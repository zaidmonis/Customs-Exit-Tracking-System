namespace CustomsExitTracking.Shared.Contracts;

public sealed record PersonDto(
    Guid PersonId,
    string NationalId,
    string FullName,
    DateOnly DateOfBirth,
    string NationalityCode,
    string? Gender);
