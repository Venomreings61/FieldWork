
using FieldWork.Application.DTOs.Beats;
using FieldWork.Application.Repositories;
using FieldWork.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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
                CenterLatitude = x.CenterLatitude,
                CenterLongitude = x.CenterLongitude,
                RadiusMeters = x.RadiusMeters,
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
                CenterLatitude = x.CenterLatitude,
                CenterLongitude = x.CenterLongitude,
                RadiusMeters = x.RadiusMeters,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}

