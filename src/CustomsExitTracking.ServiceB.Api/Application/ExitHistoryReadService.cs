using CustomsExitTracking.ServiceB.Api.Repositories;
using CustomsExitTracking.Shared.Contracts;

namespace CustomsExitTracking.ServiceB.Api.Application;

public sealed class ExitHistoryReadService(IExitRecordReadRepository repository)
{
    public Task<IReadOnlyList<ExitRecordDto>> GetByNationalIdAsync(
        string nationalId,
        ExitRecordQueryRequest request,
        CancellationToken cancellationToken) =>
        repository.GetByNationalIdAsync(nationalId, request, cancellationToken);
}
