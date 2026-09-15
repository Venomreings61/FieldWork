public class BeatResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool HasBoundaryPolygon { get; set; }
    public bool IsActive { get; set; }
}