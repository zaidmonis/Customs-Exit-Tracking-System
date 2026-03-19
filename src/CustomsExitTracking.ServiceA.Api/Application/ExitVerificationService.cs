using CustomsExitTracking.ServiceA.Api.Contracts;
using CustomsExitTracking.ServiceA.Api.Integrations;
using CustomsExitTracking.ServiceA.Api.Repositories;
using CustomsExitTracking.ServiceA.Api.Settings;
using CustomsExitTracking.Shared.Contracts;
using CustomsExitTracking.Shared.Validation;
using Microsoft.Extensions.Options;

namespace CustomsExitTracking.ServiceA.Api.Application;

public sealed class ExitVerificationService(
    IPersonReadRepository personReadRepository,
    IExitRecordReadRepository exitRecordReadRepository,
    IServiceBClient serviceBClient,
    IOptions<ScreeningRulesOptions> screeningRulesOptions)
{
    private readonly ScreeningRulesOptions options = screeningRulesOptions.Value;

    public async Task<VerifyAndInsertExitResponse> VerifyAndInsertAsync(
        string nationalId,
        VerifyAndInsertExitRequest request,
        CancellationToken cancellationToken)
    {
        var person = await personReadRepository.GetByNationalIdAsync(nationalId, cancellationToken);
        if (person is null)
        {
            return new VerifyAndInsertExitResponse(
                VerifyDecision.RejectPersonNotFound,
                false,
                0,
                "PERSON_NOT_FOUND",
                null);
        }

        var recentWindowRequest = new ExitRecordQueryRequest(
            request.DepartedAt.AddDays(-30),
            request.DepartedAt,
            null,
            SharedValidationConstants.MaxPageSize,
            0);

        var recentExits = await exitRecordReadRepository.GetByNationalIdAsync(nationalId, recentWindowRequest, cancellationToken);
        var recentExitCount = recentExits.Count;

        if (recentExitCount > options.FrequentTravelThreshold)
        {
            return new VerifyAndInsertExitResponse(
                VerifyDecision.FlagFrequentTravel,
                false,
                recentExitCount,
                "FREQUENT_TRAVEL",
                person);
        }

        await serviceBClient.CreateExitRecordAsync(nationalId, request, cancellationToken);

        return new VerifyAndInsertExitResponse(
            VerifyDecision.Pass,
            true,
            recentExitCount,
            null,
            person);
    }
}
