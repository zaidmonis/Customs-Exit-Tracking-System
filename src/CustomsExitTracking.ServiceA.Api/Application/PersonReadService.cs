using CustomsExitTracking.ServiceA.Api.Repositories;
using CustomsExitTracking.Shared.Contracts;

namespace CustomsExitTracking.ServiceA.Api.Application;

public sealed class PersonReadService(IPersonReadRepository repository)
{
    public Task<PersonDto?> GetByNationalIdAsync(string nationalId, CancellationToken cancellationToken) =>
        repository.GetByNationalIdAsync(nationalId, cancellationToken);
}
