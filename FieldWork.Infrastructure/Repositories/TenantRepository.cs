using FieldWork.Application.Repositories;
using FieldWork.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FieldWork.Infrastructure.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly FieldWorkDbContext _db;

    public TenantRepository(FieldWorkDbContext db)
    {
        _db = db;
    }

    public async Task<bool> IsFaceVerificationRequiredAsync(
    Guid tenantId,
    CancellationToken cancellationToken = default)
    {
        return await _db.Tenants
            .AsNoTracking()
            .Where(x => x.Id == tenantId)
            .Select(x => x.FaceVerificationRequired)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> SetFaceVerificationRequiredAsync(
        Guid tenantId,
        bool required,
        CancellationToken cancellationToken = default)
    {
        var tenant = await _db.Tenants
            .FirstOrDefaultAsync(x => x.Id == tenantId, cancellationToken);

        if (tenant is null)
            return false;

        tenant.FaceVerificationRequired = required;

        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }
}