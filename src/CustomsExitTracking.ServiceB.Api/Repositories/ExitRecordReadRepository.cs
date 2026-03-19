using CustomsExitTracking.ServiceB.Api.Persistence;
using CustomsExitTracking.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace CustomsExitTracking.ServiceB.Api.Repositories;

public sealed class ExitRecordReadRepository(CustomsDbContext dbContext) : IExitRecordReadRepository
{
    public async Task<IReadOnlyList<ExitRecordDto>> GetByNationalIdAsync(
        string nationalId,
        ExitRecordQueryRequest request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.ExitRecords
            .AsNoTracking()
            .Where(x => x.Person.NationalId == nationalId);

        if (request.From is not null)
        {
            query = query.Where(x => x.DepartedAt >= request.From.Value.UtcDateTime);
        }

        if (request.To is not null)
        {
            query = query.Where(x => x.DepartedAt <= request.To.Value.UtcDateTime);
        }

        if (!string.IsNullOrWhiteSpace(request.ToCountryCode))
        {
            query = query.Where(x => x.ToCountryCode == request.ToCountryCode);
        }

        var entities = await query
            .OrderByDescending(x => x.DepartedAt)
            .Skip(request.Offset)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        return entities
            .Select(x => new ExitRecordDto(
                x.ExitId,
                x.PersonId,
                new DateTimeOffset(DateTime.SpecifyKind(x.DepartedAt, DateTimeKind.Utc)),
                x.FromCountryCode,
                x.ToCountryCode,
                x.PortOfExit,
                x.TravelDocumentNumber,
                x.Purpose))
            .ToArray();
    }
}
