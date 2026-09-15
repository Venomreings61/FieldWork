namespace FieldWork.Application.DTOs.Beats;

public class UpdateBeatRequest
{
    public string Name { get; set; } = string.Empty;
    public List<CoordinateRequest> BoundaryPoints { get; set; } = [];
}