using CustomsExitTracking.ServiceA.Api.Contracts;
using CustomsExitTracking.Shared.Contracts;
using CustomsExitTracking.Shared.Validation;

namespace CustomsExitTracking.ServiceA.Tests;

public class ContractsValidationTests
{
    [Fact]
    public void CountryCodeValidation_AcceptsThreeUppercaseLetters()
    {
        Assert.True(RequestValidation.IsCountryCodeValid("SGP"));
    }

    [Fact]
    public void CountryCodeValidation_RejectsInvalidFormat()
    {
        Assert.False(RequestValidation.IsCountryCodeValid("sg"));
    }

    [Fact]
    public void PaginationValidation_RejectsValuesOutsideConfiguredRange()
    {
        var request = new ExitRecordQueryRequest(null, null, null, SharedValidationConstants.MaxPageSize + 1, -1);

        Assert.False(RequestValidation.IsPaginationValid(request));
    }

    [Fact]
    public void VerifyResponse_RetainsDecisionAndReason()
    {
        var person = new PersonDto(Guid.NewGuid(), "MY9001010001", "Ahmad Firdaus bin Rahman", new DateOnly(1990, 1, 1), "MYS", "M");
        var response = new VerifyAndInsertExitResponse(VerifyDecision.FlagFrequentTravel, false, 4, "FREQUENT_TRAVEL", person);

        Assert.Equal(VerifyDecision.FlagFrequentTravel, response.Decision);
        Assert.Equal("FREQUENT_TRAVEL", response.ReasonCode);
        Assert.False(response.InsertPerformed);
        Assert.Equal(person, response.Person);
    }
}
