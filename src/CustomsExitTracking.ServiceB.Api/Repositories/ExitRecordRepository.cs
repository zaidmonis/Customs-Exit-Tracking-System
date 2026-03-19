using CustomsExitTracking.ServiceB.Api.Contracts;
using CustomsExitTracking.ServiceB.Api.Persistence;
using CustomsExitTracking.ServiceB.Api.Persistence.Entities;
using CustomsExitTracking.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace CustomsExitTracking.ServiceB.Api.Repositories;

public sealed class ExitRecordRepository(CustomsDbContext dbContext) : IExitRecordRepository
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

    public async Task<ExitRecordDto> CreateAsync(
        Guid personId,
        ExitRecordCreateRequest request,
        CancellationToken cancellationToken)
    {
        var entity = new ExitRecordEntity
        {
            ExitId = Guid.NewGuid(),
            PersonId = personId,
            DepartedAt = request.DepartedAt.UtcDateTime,
            FromCountryCode = request.FromCountryCode,
            ToCountryCode = request.ToCountryCode,
            PortOfExit = request.PortOfExit,
            TravelDocumentNumber = request.TravelDocumentNumber,
            Purpose = request.Purpose,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.ExitRecords.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ExitRecordDto(
            entity.ExitId,
            entity.PersonId,
            new DateTimeOffset(DateTime.SpecifyKind(entity.DepartedAt, DateTimeKind.Utc)),
            entity.FromCountryCode,
            entity.ToCountryCode,
            entity.PortOfExit,
            entity.TravelDocumentNumber,
            entity.Purpose);
    }

    public async Task<ExitRecordDto?> UpdateAsync(
        Guid personId,
        ExitRecordUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.ExitRecords
            .SingleOrDefaultAsync(x => x.PersonId == personId && x.ExitId == request.ExitId, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.DepartedAt = request.DepartedAt.UtcDateTime;
        entity.FromCountryCode = request.FromCountryCode;
        entity.ToCountryCode = request.ToCountryCode;
        entity.PortOfExit = request.PortOfExit;
        entity.TravelDocumentNumber = request.TravelDocumentNumber;
        entity.Purpose = request.Purpose;
        entity.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return new ExitRecordDto(
            entity.ExitId,
            entity.PersonId,
            new DateTimeOffset(DateTime.SpecifyKind(entity.DepartedAt, DateTimeKind.Utc)),
            entity.FromCountryCode,
            entity.ToCountryCode,
            entity.PortOfExit,
            entity.TravelDocumentNumber,
            entity.Purpose);
    }

    public async Task<bool> DeleteAsync(
        Guid personId,
        Guid exitId,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.ExitRecords
            .SingleOrDefaultAsync(x => x.PersonId == personId && x.ExitId == exitId, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        dbContext.ExitRecords.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
