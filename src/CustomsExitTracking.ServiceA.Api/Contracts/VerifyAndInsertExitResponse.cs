using CustomsExitTracking.Shared.Contracts;

namespace CustomsExitTracking.ServiceA.Api.Contracts;

public sealed record VerifyAndInsertExitResponse(
    VerifyDecision Decision,
    bool InsertPerformed,
    int RecentExitCount,
    string? ReasonCode,
    PersonDto? Person);
