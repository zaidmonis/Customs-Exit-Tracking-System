namespace CustomsExitTracking.ServiceA.Api.Contracts;

public sealed record HealthStatusResponse(
    string Service,
    string Status,
    DateTimeOffset CheckedAtUtc);
