namespace CustomsExitTracking.ServiceB.Api.Contracts;

public sealed record ExitRecordUpdateRequest(
    Guid ExitId,
    DateTimeOffset DepartedAt,
    string FromCountryCode,
    string ToCountryCode,
    string PortOfExit,
    string? TravelDocumentNumber,
    string? Purpose);
