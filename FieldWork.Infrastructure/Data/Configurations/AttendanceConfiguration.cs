using FieldWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> builder)
    {
        builder.ToTable("attendance");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ClientAttendanceId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Action)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.RecordedAt).IsRequired();
        builder.Property(x => x.ReceivedAt).IsRequired();

        builder.Property(x => x.Latitude).HasPrecision(10, 7).IsRequired();
        builder.Property(x => x.Longitude).HasPrecision(10, 7).IsRequired();
        builder.Property(x => x.AccuracyMeters).HasPrecision(10, 2);

        builder.Property(x => x.Source)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.SyncStatus)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.IsWithinGeofence).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Beat>()
            .WithMany()
            .HasForeignKey(x => x.BeatId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ClientAttendanceId).IsUnique();
       // builder.HasIndex(x => new { x.EmployeeId, x.RecordedAt });
        builder.HasIndex(x => new { x.EmployeeId, x.ReceivedAt });
    }
}