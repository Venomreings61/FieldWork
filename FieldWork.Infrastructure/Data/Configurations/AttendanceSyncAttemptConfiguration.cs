using FieldWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FieldWork.Infrastructure.Data.Configurations;

public class AttendanceSyncAttemptConfiguration
    : IEntityTypeConfiguration<AttendanceSyncAttempt>
{
    public void Configure(EntityTypeBuilder<AttendanceSyncAttempt> builder)
    {
        builder.ToTable("attendance_sync_attempts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AttemptedAt)
            .IsRequired();

        builder.Property(x => x.IsSuccessful)
            .IsRequired();

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(2000);

        builder.HasOne<Attendance>()
            .WithMany()
            .HasForeignKey(x => x.AttendanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.AttendanceId, x.AttemptedAt });
    }
}