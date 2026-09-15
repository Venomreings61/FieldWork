using FieldWork.Application.DTOs.Beats;
using FieldWork.Application.Exceptions;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace FieldWork.Application.Services;

public static class BeatGeometryValidator
{
    private static readonly GeometryFactory Factory =
        NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

    public static Polygon BuildAndValidate(List<CoordinateRequest> points)
    {
        if (points is null || points.Count < 3)
        {
            throw new BusinessRuleException(
                "BEAT_INSUFFICIENT_BOUNDARY_POINTS",
                "A beat boundary requires at least 3 distinct points.");
        }

        foreach (var p in points)
        {
            if (p.Latitude is < -90 or > 90)
            {
                throw new BusinessRuleException(
                    "BEAT_INVALID_COORDINATE",
                    $"Latitude {p.Latitude} is out of range (-90 to 90).");
            }

            if (p.Longitude is < -180 or > 180)
            {
                throw new BusinessRuleException(
                    "BEAT_INVALID_COORDINATE",
                    $"Longitude {p.Longitude} is out of range (-180 to 180).");
            }
        }

        var coordinates = points
            .Select(p => new Coordinate((double)p.Longitude, (double)p.Latitude))
            .ToList();

        // Auto-close the ring if the caller didn't repeat the first point as the last.
        if (!coordinates.First().Equals2D(coordinates.Last()))
        {
            coordinates.Add(coordinates.First());
        }

        // Distinct-point check happens after closing, so a ring with only the
        // auto-added closing point doesn't silently satisfy the minimum.
        var distinctCount = coordinates.Take(coordinates.Count - 1).Distinct().Count();
        if (distinctCount < 3)
        {
            throw new BusinessRuleException(
                "BEAT_INSUFFICIENT_BOUNDARY_POINTS",
                "A beat boundary requires at least 3 distinct points.");
        }

        var ring = Factory.CreateLinearRing(coordinates.ToArray());
        var polygon = Factory.CreatePolygon(ring);

        if (!polygon.IsValid)
        {
            throw new BusinessRuleException(
                "BEAT_INVALID_POLYGON",
                "The boundary points do not form a valid polygon (self-intersecting or malformed).");
        }

        return polygon;
    }
}