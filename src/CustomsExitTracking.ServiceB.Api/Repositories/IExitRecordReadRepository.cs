using CustomsExitTracking.Shared.Contracts;

namespace CustomsExitTracking.ServiceB.Api.Repositories;

public interface IExitRecordReadRepository
{
    Task<IReadOnlyList<ExitRecordDto>> GetByNationalIdAsync(
        string nationalId,
        ExitRecordQueryRequest request,
        CancellationToken cancellationToken);
}
