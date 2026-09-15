namespace FieldWork.Application.DTOs.Beats;

public class CreateBeatRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<CoordinateRequest> BoundaryPoints { get; set; } = [];
}