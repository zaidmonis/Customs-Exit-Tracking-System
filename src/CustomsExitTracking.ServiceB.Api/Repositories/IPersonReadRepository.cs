using CustomsExitTracking.Shared.Contracts;

namespace CustomsExitTracking.ServiceB.Api.Repositories;

public interface IPersonReadRepository
{
    Task<PersonDto?> GetByNationalIdAsync(string nationalId, CancellationToken cancellationToken);
}
