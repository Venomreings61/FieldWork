using FieldWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FieldWork.Infrastructure.Data.Configurations;

public class FaceEmbeddingConfiguration : IEntityTypeConfiguration<FaceEmbedding>
{
    public void Configure(EntityTypeBuilder<FaceEmbedding> builder)
    {
        builder.ToTable("employee_face_embeddings");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Embedding)
            .HasColumnType("vector(512)")
            .IsRequired();

        builder.Property(e => e.Model)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.ModelVersion)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .IsRequired();

        // 1:0..1 Relationship with Employee
        builder.HasOne(e => e.Employee)
            .WithOne(emp => emp.FaceEmbedding)
            .HasForeignKey<FaceEmbedding>(e => e.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.EmployeeId)
            .IsUnique();
    }
}