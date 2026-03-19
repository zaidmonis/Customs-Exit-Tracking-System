namespace CustomsExitTracking.ServiceA.Api.Persistence.Entities;

public sealed class PersonIdentityEntity
{
    public Guid PersonId { get; set; }

    public string NationalId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public string NationalityCode { get; set; } = string.Empty;

    public string? Gender { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<ExitRecordEntity> ExitRecords { get; set; } = [];
}
