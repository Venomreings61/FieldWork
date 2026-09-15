using NetTopologySuite.Geometries;

namespace FieldWork.Domain.Entities;

public class Beat
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;

    public Polygon BoundaryPolygon { get; set; } = null!;

    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}