using CustomsExitTracking.ServiceA.Api.Repositories;
using CustomsExitTracking.Shared.Contracts;

namespace CustomsExitTracking.ServiceA.Api.Application;

public sealed class ExitRecordReadService(IExitRecordReadRepository repository)
{
    public Task<IReadOnlyList<ExitRecordDto>> GetByNationalIdAsync(
        string nationalId,
        ExitRecordQueryRequest request,
        CancellationToken cancellationToken) =>
        repository.GetByNationalIdAsync(nationalId, request, cancellationToken);
}
