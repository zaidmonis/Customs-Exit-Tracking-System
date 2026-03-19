namespace CustomsExitTracking.ServiceB.Api.Persistence.Entities;

public sealed class ExitRecordEntity
{
    public Guid ExitId { get; set; }

    public Guid PersonId { get; set; }

    public DateTime DepartedAt { get; set; }

    public string FromCountryCode { get; set; } = string.Empty;

    public string ToCountryCode { get; set; } = string.Empty;

    public string PortOfExit { get; set; } = string.Empty;

    public string? TravelDocumentNumber { get; set; }

    public string? Purpose { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public PersonIdentityEntity Person { get; set; } = null!;
}
