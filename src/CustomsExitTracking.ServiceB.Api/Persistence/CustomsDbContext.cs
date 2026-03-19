using CustomsExitTracking.ServiceB.Api.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomsExitTracking.ServiceB.Api.Persistence;

public sealed class CustomsDbContext(DbContextOptions<CustomsDbContext> options) : DbContext(options)
{
    public DbSet<PersonIdentityEntity> Persons => Set<PersonIdentityEntity>();

    public DbSet<ExitRecordEntity> ExitRecords => Set<ExitRecordEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PersonIdentityEntity>(entity =>
        {
            entity.ToTable("person_identity");
            entity.HasKey(x => x.PersonId);

            entity.Property(x => x.PersonId).HasColumnName("person_id").HasColumnType("RAW(16)");
            entity.Property(x => x.NationalId).HasColumnName("national_id").HasMaxLength(50).IsRequired();
            entity.Property(x => x.FullName).HasColumnName("full_name").HasMaxLength(200).IsRequired();
            entity.Property(x => x.DateOfBirth).HasColumnName("date_of_birth").IsRequired();
            entity.Property(x => x.NationalityCode).HasColumnName("nationality_code").HasMaxLength(3).IsRequired();
            entity.Property(x => x.Gender).HasColumnName("gender").HasMaxLength(20);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(x => x.NationalId).IsUnique();
        });

        modelBuilder.Entity<ExitRecordEntity>(entity =>
        {
            entity.ToTable("exit_record");
            entity.HasKey(x => x.ExitId);

            entity.Property(x => x.ExitId).HasColumnName("exit_id").HasColumnType("RAW(16)");
            entity.Property(x => x.PersonId).HasColumnName("person_id").HasColumnType("RAW(16)");
            entity.Property(x => x.DepartedAt).HasColumnName("departed_at").IsRequired();
            entity.Property(x => x.FromCountryCode).HasColumnName("from_country_code").HasMaxLength(3).IsRequired();
            entity.Property(x => x.ToCountryCode).HasColumnName("to_country_code").HasMaxLength(3).IsRequired();
            entity.Property(x => x.PortOfExit).HasColumnName("port_of_exit").HasMaxLength(120).IsRequired();
            entity.Property(x => x.TravelDocumentNumber).HasColumnName("travel_doc_no").HasMaxLength(50);
            entity.Property(x => x.Purpose).HasColumnName("purpose").HasMaxLength(80);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            entity.HasOne(x => x.Person)
                .WithMany(x => x.ExitRecords)
                .HasForeignKey(x => x.PersonId);
        });
    }
}
