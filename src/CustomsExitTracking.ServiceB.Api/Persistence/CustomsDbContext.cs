using CustomsExitTracking.ServiceB.Api.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomsExitTracking.ServiceB.Api.Persistence;

public sealed class CustomsDbContext(DbContextOptions<CustomsDbContext> options) : DbContext(options)
{
    public DbSet<PersonIdentityEntity> Persons => Set<PersonIdentityEntity>();

    public DbSet<ExitRecordEntity> ExitRecords => Set<ExitRecordEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("CUSTOMS_APP");

        modelBuilder.Entity<PersonIdentityEntity>(entity =>
        {
            entity.ToTable("PERSON_IDENTITY");
            entity.HasKey(x => x.PersonId);

            entity.Property(x => x.PersonId).HasColumnName("PERSON_ID").HasColumnType("RAW(16)");
            entity.Property(x => x.NationalId).HasColumnName("NATIONAL_ID").HasMaxLength(50).IsRequired();
            entity.Property(x => x.FullName).HasColumnName("FULL_NAME").HasMaxLength(200).IsRequired();
            entity.Property(x => x.DateOfBirth).HasColumnName("DATE_OF_BIRTH").IsRequired();
            entity.Property(x => x.NationalityCode).HasColumnName("NATIONALITY_CODE").HasMaxLength(3).IsRequired();
            entity.Property(x => x.Gender).HasColumnName("GENDER").HasMaxLength(20);
            entity.Property(x => x.CreatedAt).HasColumnName("CREATED_AT");
            entity.Property(x => x.UpdatedAt).HasColumnName("UPDATED_AT");

            entity.HasIndex(x => x.NationalId).IsUnique();
        });

        modelBuilder.Entity<ExitRecordEntity>(entity =>
        {
            entity.ToTable("EXIT_RECORD");
            entity.HasKey(x => x.ExitId);

            entity.Property(x => x.ExitId).HasColumnName("EXIT_ID").HasColumnType("RAW(16)");
            entity.Property(x => x.PersonId).HasColumnName("PERSON_ID").HasColumnType("RAW(16)");
            entity.Property(x => x.DepartedAt).HasColumnName("DEPARTED_AT").IsRequired();
            entity.Property(x => x.FromCountryCode).HasColumnName("FROM_COUNTRY_CODE").HasMaxLength(3).IsRequired();
            entity.Property(x => x.ToCountryCode).HasColumnName("TO_COUNTRY_CODE").HasMaxLength(3).IsRequired();
            entity.Property(x => x.PortOfExit).HasColumnName("PORT_OF_EXIT").HasMaxLength(120).IsRequired();
            entity.Property(x => x.TravelDocumentNumber).HasColumnName("TRAVEL_DOC_NO").HasMaxLength(50);
            entity.Property(x => x.Purpose).HasColumnName("PURPOSE").HasMaxLength(80);
            entity.Property(x => x.CreatedAt).HasColumnName("CREATED_AT");
            entity.Property(x => x.UpdatedAt).HasColumnName("UPDATED_AT");

            entity.HasOne(x => x.Person)
                .WithMany(x => x.ExitRecords)
                .HasForeignKey(x => x.PersonId);
        });
    }
}
