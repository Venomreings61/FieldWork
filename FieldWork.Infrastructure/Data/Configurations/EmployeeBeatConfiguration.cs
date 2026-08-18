using FieldWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FieldWork.Infrastructure.Data.Configurations;

public class EmployeeBeatConfiguration : IEntityTypeConfiguration<EmployeeBeat>
{
    public void Configure(EntityTypeBuilder<EmployeeBeat> builder)
    {
        builder.ToTable("employee_beats");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AssignedFrom)
            .IsRequired();

        builder.Property(x => x.AssignedTo);

        builder.Property(x => x.IsActive)
            .IsRequired();

      


builder.HasOne(x => x.Employee)
    .WithMany()
    .HasForeignKey(x => x.EmployeeId)
    .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Beat)
            .WithMany()
            .HasForeignKey(x => x.BeatId)
            .OnDelete(DeleteBehavior.Restrict);




        builder.HasIndex(x => new { x.EmployeeId, x.BeatId });

        builder.HasIndex(x => x.EmployeeId)
    .IsUnique()
    .HasFilter("\"IsActive\" = true");


       
    }
}