namespace FieldWork.Application.Repositories;

public interface IGeofenceRepository
{
    Task<bool> IsPointWithinBeatPolygonAsync(
        Guid beatId,
        decimal latitude,
        decimal longitude,
        CancellationToken cancellationToken = default);
}