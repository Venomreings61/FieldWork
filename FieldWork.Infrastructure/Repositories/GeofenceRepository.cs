using FieldWork.Application.Repositories;
using FieldWork.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FieldWork.Infrastructure.Repositories;

public class GeofenceRepository : IGeofenceRepository
{
    private readonly FieldWorkDbContext _db;

    public GeofenceRepository(FieldWorkDbContext db)
    {
        _db = db;
    }

    public async Task<bool> IsPointWithinBeatPolygonAsync(
        Guid beatId,
        decimal latitude,
        decimal longitude,
        CancellationToken cancellationToken = default)
    {
        // NOTE: PostGIS point order is (longitude, latitude) — X, Y — the
        // opposite of how we normally say "lat/lng". Getting this backwards
        // is the single most common PostGIS bug; do not "simplify" this call.
        var lng = (double)longitude;
        var lat = (double)latitude;

        // In EF Core 7+, Database.SqlQuery<T> taking FormattableString ($"...")
        // automatically parameterizes string interpolation safely.
        var result = await _db.Database
            .SqlQuery<bool>($@"
                SELECT ST_Covers(
                    boundary_polygon,
                    ST_SetSRID(ST_MakePoint({lng}, {lat}), 4326)
                ) AS ""Value""
                FROM beats
                WHERE ""Id"" = {beatId} AND boundary_polygon IS NOT NULL")
            .SingleOrDefaultAsync(cancellationToken);

        return result;
    }
}