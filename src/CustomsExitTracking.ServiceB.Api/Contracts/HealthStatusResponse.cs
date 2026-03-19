namespace CustomsExitTracking.ServiceB.Api.Contracts;

public sealed record HealthStatusResponse(
    string Service,
    string Status,
    DateTimeOffset CheckedAtUtc);
