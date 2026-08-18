using FieldWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FieldWork.Infrastructure.Data.Configurations;

public class BeatConfiguration : IEntityTypeConfiguration<Beat>
{
    public void Configure(EntityTypeBuilder<Beat> builder)
    {
        builder.ToTable("beats");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.CenterLatitude)
            .HasPrecision(9, 6)
            .IsRequired();

        builder.Property(x => x.CenterLongitude)
            .HasPrecision(9, 6)
            .IsRequired();

        builder.Property(x => x.RadiusMeters)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.Code })
            .IsUnique();

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}