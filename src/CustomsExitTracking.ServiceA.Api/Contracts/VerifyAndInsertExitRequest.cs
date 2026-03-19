namespace CustomsExitTracking.ServiceA.Api.Contracts;

public sealed record VerifyAndInsertExitRequest(
    DateTimeOffset DepartedAt,
    string FromCountryCode,
    string ToCountryCode,
    string PortOfExit,
    string? TravelDocumentNumber,
    string? Purpose);
