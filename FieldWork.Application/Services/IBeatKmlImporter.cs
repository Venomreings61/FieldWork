using FieldWork.Application.DTOs.Beats;

namespace FieldWork.Application.Services;

public interface IBeatKmlImporter
{
    ImportedBeatGeometry Parse(Stream kmlStream);
}

public class ImportedBeatGeometry
{
    public string? SuggestedName { get; set; }
    public List<CoordinateRequest> BoundaryPoints { get; set; } = [];
    public Dictionary<string, string> Metadata { get; set; } = [];
}