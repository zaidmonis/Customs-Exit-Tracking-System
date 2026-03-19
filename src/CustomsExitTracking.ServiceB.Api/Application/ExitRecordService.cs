using CustomsExitTracking.ServiceB.Api.Contracts;
using CustomsExitTracking.ServiceB.Api.Repositories;
using CustomsExitTracking.Shared.Contracts;

namespace CustomsExitTracking.ServiceB.Api.Application;

public sealed class ExitRecordService(
    IPersonReadRepository personReadRepository,
    IExitRecordRepository exitRecordRepository)
{
    public Task<IReadOnlyList<ExitRecordDto>> GetByNationalIdAsync(
        string nationalId,
        ExitRecordQueryRequest request,
        CancellationToken cancellationToken) =>
        exitRecordRepository.GetByNationalIdAsync(nationalId, request, cancellationToken);

    public async Task<ExitRecordDto?> CreateAsync(
        string nationalId,
        ExitRecordCreateRequest request,
        CancellationToken cancellationToken)
    {
        var person = await personReadRepository.GetByNationalIdAsync(nationalId, cancellationToken);
        if (person is null)
        {
            return null;
        }

        return await exitRecordRepository.CreateAsync(person.PersonId, request, cancellationToken);
    }
}
