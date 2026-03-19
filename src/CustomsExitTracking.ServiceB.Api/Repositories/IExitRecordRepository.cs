using CustomsExitTracking.ServiceB.Api.Contracts;
using CustomsExitTracking.Shared.Contracts;

namespace CustomsExitTracking.ServiceB.Api.Repositories;

public interface IExitRecordRepository
{
    Task<IReadOnlyList<ExitRecordDto>> GetByNationalIdAsync(
        string nationalId,
        ExitRecordQueryRequest request,
        CancellationToken cancellationToken);

    Task<ExitRecordDto> CreateAsync(
        Guid personId,
        ExitRecordCreateRequest request,
        CancellationToken cancellationToken);

    Task<ExitRecordDto?> UpdateAsync(
        Guid personId,
        ExitRecordUpdateRequest request,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(
        Guid personId,
        Guid exitId,
        CancellationToken cancellationToken);
}
