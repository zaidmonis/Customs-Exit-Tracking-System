namespace CustomsExitTracking.Shared.Contracts;

public sealed record ExitRecordQueryRequest(
    DateTimeOffset? From,
    DateTimeOffset? To,
    string? ToCountryCode,
    int Limit = 50,
    int Offset = 0);
