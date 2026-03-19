using CustomsExitTracking.Shared.Contracts;

namespace CustomsExitTracking.ServiceA.Api.Repositories;

public interface IPersonReadRepository
{
    Task<PersonDto?> GetByNationalIdAsync(string nationalId, CancellationToken cancellationToken);
}
