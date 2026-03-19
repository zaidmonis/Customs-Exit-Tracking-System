namespace CustomsExitTracking.Shared.Contracts;

public sealed record ExitRecordDto(
    Guid ExitId,
    Guid PersonId,
    DateTimeOffset DepartedAt,
    string FromCountryCode,
    string ToCountryCode,
    string PortOfExit,
    string? TravelDocumentNumber,
    string? Purpose);
