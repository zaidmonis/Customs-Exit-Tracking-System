using CustomsExitTracking.Shared.Contracts;

namespace CustomsExitTracking.ServiceA.Api.Repositories;

public interface IExitRecordReadRepository
{
    Task<IReadOnlyList<ExitRecordDto>> GetByNationalIdAsync(
        string nationalId,
        ExitRecordQueryRequest request,
        CancellationToken cancellationToken);
}
