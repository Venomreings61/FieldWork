using FieldWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.Code })
            .IsUnique();

        builder.Property(x => x.BoundaryPolygon)
            .HasColumnName("boundary_polygon")
            .HasColumnType("geometry(Polygon, 4326)")
            .IsRequired();

        builder.HasIndex(x => x.BoundaryPolygon)
            .HasMethod("GIST");

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}