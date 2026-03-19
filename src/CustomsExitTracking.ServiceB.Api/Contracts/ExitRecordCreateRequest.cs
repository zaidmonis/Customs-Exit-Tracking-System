namespace CustomsExitTracking.ServiceB.Api.Contracts;

public sealed record ExitRecordCreateRequest(
    DateTimeOffset DepartedAt,
    string FromCountryCode,
    string ToCountryCode,
    string PortOfExit,
    string? TravelDocumentNumber,
    string? Purpose);
