namespace FieldWork.Application.Services;

public interface IGeofenceService
{
    bool IsWithinRadius(
        decimal latitude,
        decimal longitude,
        decimal centerLatitude,
        decimal centerLongitude,
        decimal radiusMeters);
}