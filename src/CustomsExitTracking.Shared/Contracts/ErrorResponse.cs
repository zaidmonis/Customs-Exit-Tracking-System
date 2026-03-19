namespace CustomsExitTracking.Shared.Contracts;

public sealed record ErrorResponse(
    string Code,
    string Message,
    IReadOnlyDictionary<string, string[]>? Errors = null);
