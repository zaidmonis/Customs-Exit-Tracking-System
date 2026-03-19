namespace CustomsExitTracking.Shared.Validation;

public static class SharedValidationConstants
{
    public const int DefaultPageSize = 50;
    public const int MaxPageSize = 200;
    public const int IsoCountryCodeLength = 3;
    public static readonly string[] ImmutableExitFields = ["exit_id", "person_id", "created_at"];
}
