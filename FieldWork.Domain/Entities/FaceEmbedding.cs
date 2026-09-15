using Pgvector;
using Vector = Pgvector.Vector;

namespace FieldWork.Domain.Entities;

public class FaceEmbedding
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public Vector Embedding { get; set; } = null!;

    public string Model { get; set; } = string.Empty;
    public string ModelVersion { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}