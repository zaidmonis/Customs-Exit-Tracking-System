using CustomsExitTracking.ServiceA.Api.Contracts;
using CustomsExitTracking.Shared.Contracts;

namespace CustomsExitTracking.ServiceA.Api.Integrations;

public interface IServiceBClient
{
    Task<ExitRecordDto> CreateExitRecordAsync(
        string nationalId,
        VerifyAndInsertExitRequest request,
        CancellationToken cancellationToken);
}
