using System.Text.RegularExpressions;
using CustomsExitTracking.Shared.Contracts;

namespace CustomsExitTracking.Shared.Validation;

public static partial class RequestValidation
{
    [GeneratedRegex("^[A-Z]{3}$")]
    private static partial Regex CountryCodeRegex();

    public static bool IsNationalIdValid(string? nationalId) =>
        !string.IsNullOrWhiteSpace(nationalId);

    public static bool IsCountryCodeValid(string? countryCode) =>
        !string.IsNullOrWhiteSpace(countryCode) &&
        CountryCodeRegex().IsMatch(countryCode);

    public static bool IsPaginationValid(ExitRecordQueryRequest request) =>
        request.Offset >= 0 &&
        request.Limit is >= 1 and <= SharedValidationConstants.MaxPageSize;
}
