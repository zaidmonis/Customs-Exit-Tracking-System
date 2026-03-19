using CustomsExitTracking.ServiceB.Api.Contracts;
using CustomsExitTracking.Shared.Contracts;
using CustomsExitTracking.Shared.Validation;

namespace CustomsExitTracking.ServiceB.Tests;

public class ContractsValidationTests
{
    [Fact]
    public void CountryCodeValidation_AcceptsThreeUppercaseLetters()
    {
        Assert.True(RequestValidation.IsCountryCodeValid("MYS"));
    }

    [Fact]
    public void PaginationValidation_RejectsNegativeOffset()
    {
        var request = new ExitRecordQueryRequest(null, null, null, 25, -1);

        Assert.False(RequestValidation.IsPaginationValid(request));
    }

    [Fact]
    public void ImmutableExitFields_AreExposedForUpdateRules()
    {
        Assert.Contains("exit_id", SharedValidationConstants.ImmutableExitFields);
        Assert.Contains("person_id", SharedValidationConstants.ImmutableExitFields);
        Assert.Contains("created_at", SharedValidationConstants.ImmutableExitFields);
    }

    [Fact]
    public void ErrorResponse_CanCarryValidationErrors()
    {
        var response = new ErrorResponse(
            "VALIDATION_ERROR",
            "One or more validation errors occurred.",
            new Dictionary<string, string[]>
            {
                ["toCountryCode"] = ["Must be a valid ISO alpha-3 code."]
            });

        Assert.Equal("VALIDATION_ERROR", response.Code);
        Assert.NotNull(response.Errors);
        Assert.Contains("toCountryCode", response.Errors!.Keys);
    }
}
