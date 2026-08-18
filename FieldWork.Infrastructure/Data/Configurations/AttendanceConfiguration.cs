using FieldWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FieldWork.Infrastructure.Data.Configurations;

public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> builder)
    {
        builder.ToTable("attendances");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ClientAttendanceId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Action)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Source)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.SyncStatus)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Latitude)
            .HasPrecision(9, 6)
            .IsRequired();

        builder.Property(x => x.Longitude)
            .HasPrecision(9, 6)
            .IsRequired();

        builder.Property(x => x.AccuracyMeters)
            .HasPrecision(10, 2);

        builder.Property(x => x.RecordedAt)
            .IsRequired();

        builder.Property(x => x.ReceivedAt)
            .IsRequired();

        builder.Property(x => x.IsWithinGeofence)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.ClientAttendanceId)
            .IsUnique();

        builder.HasIndex(x => new { x.EmployeeId, x.RecordedAt });

        builder.HasIndex(x => new { x.BeatId, x.RecordedAt });

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Beat>()
            .WithMany()
            .HasForeignKey(x => x.BeatId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}