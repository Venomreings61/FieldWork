
//using FieldWork.Application.DTOs.Beats;
using FieldWork.Application.Exceptions;
using FieldWork.Application.Repositories;
using FieldWork.Domain.Entities;
using FieldWork.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace FieldWork.Infrastructure.Repositories;

public class BeatRepository : IBeatRepository
{
    private readonly FieldWorkDbContext _db;

    public BeatRepository(FieldWorkDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<BeatResponse>> GetAllAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await _db.Beats
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.Code)
            .Select(x => new BeatResponse
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,           
                HasBoundaryPolygon = x.BoundaryPolygon != null,   // ← new
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

       
    }

    public async Task<BeatResponse?> GetByIdAsync(
     Guid beatId,
     Guid tenantId,
     CancellationToken cancellationToken = default)
    {
        return await _db.Beats
            .AsNoTracking()
            .Where(x =>
                x.Id == beatId &&
                x.TenantId == tenantId)
            .Select(x => new BeatResponse
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
                HasBoundaryPolygon = x.BoundaryPolygon != null,   // ← new
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<BeatResponse> CreateAsync(
    Guid tenantId,
    string code,
    string name,
    Polygon boundaryPolygon,
    CancellationToken cancellationToken = default)
    {
        var beat = new Beat
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Code = code,
            Name = name,
            BoundaryPolygon = boundaryPolygon,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.Beats.Add(beat);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            _db.Entry(beat).State = EntityState.Detached;
            throw new ConflictException("BEAT_CODE_ALREADY_EXISTS", "A beat with this code already exists for this tenant.");
        }

        return new BeatResponse
        {
            Id = beat.Id,
            Name = beat.Name,
            Code = beat.Code,
            HasBoundaryPolygon = true,
            IsActive = beat.IsActive
        };
    }

    public async Task<BeatResponse?> UpdateAsync(
        Guid beatId,
        Guid tenantId,
        string name,
        Polygon boundaryPolygon,
        CancellationToken cancellationToken = default)
    {
        var beat = await _db.Beats
            .FirstOrDefaultAsync(x => x.Id == beatId && x.TenantId == tenantId, cancellationToken);

        if (beat is null)
        {
            return null;
        }

        beat.Name = name;
        beat.BoundaryPolygon = boundaryPolygon;

        await _db.SaveChangesAsync(cancellationToken);

        return new BeatResponse
        {
            Id = beat.Id,
            Name = beat.Name,
            Code = beat.Code,
            HasBoundaryPolygon = beat.BoundaryPolygon != null,
            IsActive = beat.IsActive
        };
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505";
    }
}

