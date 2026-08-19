namespace FieldWork.Application.Services;

public class GeofenceService : IGeofenceService
{
    public bool IsWithinRadius(
        decimal latitude,
        decimal longitude,
        decimal centerLatitude,
        decimal centerLongitude,
        decimal radiusMeters)
    {
        var earthRadiusMeters = 6_371_000d;

        var lat1 = DegreesToRadians((double)centerLatitude);
        var lat2 = DegreesToRadians((double)latitude);

        var deltaLat = DegreesToRadians(
            (double)(latitude - centerLatitude));

        var deltaLon = DegreesToRadians(
            (double)(longitude - centerLongitude));

        var a =
            Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
            Math.Cos(lat1) *
            Math.Cos(lat2) *
            Math.Sin(deltaLon / 2) *
            Math.Sin(deltaLon / 2);

        var c = 2 * Math.Atan2(
            Math.Sqrt(a),
            Math.Sqrt(1 - a));

        var distanceMeters = earthRadiusMeters * c;

        return distanceMeters <= (double)radiusMeters;
    }

    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180d;
    }
}