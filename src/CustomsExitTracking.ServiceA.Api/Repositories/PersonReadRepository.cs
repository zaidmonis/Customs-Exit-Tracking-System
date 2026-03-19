using CustomsExitTracking.ServiceA.Api.Persistence;
using CustomsExitTracking.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace CustomsExitTracking.ServiceA.Api.Repositories;

public sealed class PersonReadRepository(CustomsDbContext dbContext) : IPersonReadRepository
{
    public async Task<PersonDto?> GetByNationalIdAsync(string nationalId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Persons
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.NationalId == nationalId, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        return new PersonDto(
            entity.PersonId,
            entity.NationalId,
            entity.FullName,
            DateOnly.FromDateTime(entity.DateOfBirth),
            entity.NationalityCode,
            entity.Gender);
    }
}
