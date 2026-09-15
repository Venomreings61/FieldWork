using FieldWork.Application.DTOs.Beats;
using FieldWork.Application.Exceptions;
using FieldWork.Application.Services;
using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;

namespace FieldWork.Infrastructure.Services;

public class KmlBeatImporter : IBeatKmlImporter
{
    public ImportedBeatGeometry Parse(Stream kmlStream)
    {
        KmlFile kmlFile;
        try
        {
            kmlFile = KmlFile.Load(kmlStream);
        }
        catch (Exception ex)
        {
            throw new BusinessRuleException("KML_PARSE_FAILED", $"Could not parse KML file: {ex.Message}");
        }

        var placemark = kmlFile.Root.Flatten().OfType<Placemark>().FirstOrDefault();
        if (placemark is null)
        {
            throw new BusinessRuleException("KML_NO_PLACEMARK", "KML file contains no Placemark.");
        }

        var polygon = placemark.Geometry as SharpKml.Dom.Polygon;
        if (polygon?.OuterBoundary?.LinearRing?.Coordinates is null)
        {
            throw new BusinessRuleException("KML_NO_POLYGON", "Placemark does not contain a Polygon with an outer boundary.");
        }

        var points = polygon.OuterBoundary.LinearRing.Coordinates
            .Select(c => new CoordinateRequest
            {
                Latitude = (decimal)c.Latitude,
                Longitude = (decimal)c.Longitude
            })
            .ToList();

        var metadata = new Dictionary<string, string>();
        if (placemark.ExtendedData is not null)
        {
            foreach (var schemaData in placemark.ExtendedData.SchemaData)
            {
                foreach (var simpleData in schemaData.SimpleData)
                {
                    metadata[simpleData.Name] = simpleData.Text;
                }
            }
        }

        metadata.TryGetValue("Beat", out var suggestedName);

        return new ImportedBeatGeometry
        {
            SuggestedName = suggestedName ?? placemark.Name,
            BoundaryPoints = points,
            Metadata = metadata
        };
    }
}