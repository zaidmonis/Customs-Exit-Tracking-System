using CustomsExitTracking.ServiceB.Api.Repositories;
using CustomsExitTracking.Shared.Contracts;

namespace CustomsExitTracking.ServiceB.Api.Application;

public sealed class PersonReadService(IPersonReadRepository repository)
{
    public Task<PersonDto?> GetByNationalIdAsync(string nationalId, CancellationToken cancellationToken) =>
        repository.GetByNationalIdAsync(nationalId, cancellationToken);
}
