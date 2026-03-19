namespace CustomsExitTracking.Shared.Contracts;

public sealed record HealthStatusResponse(
    string Service,
    string Status,
    DateTimeOffset CheckedAtUtc);
