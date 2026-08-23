using FieldWork.Application.DTOs.Employees;
using FieldWork.Application.Repositories;
using FieldWork.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FieldWork.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly FieldWorkDbContext _db;

    public EmployeeRepository(FieldWorkDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<EmployeeResponse>> GetAllAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await _db.Employees
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.EmployeeCode)
            .Select(x => new EmployeeResponse
            {
                Id = x.Id,
                EmployeeCode = x.EmployeeCode,
                FirstName = x.FirstName,
                LastName = x.LastName,
                PhoneNumber = x.PhoneNumber,
                IsActive = x.IsActive,
                Username = x.User.Username
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<EmployeeResponse?> GetByIdAsync(
        Guid employeeId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await _db.Employees
            .AsNoTracking()
            .Where(x =>
                x.Id == employeeId &&
                x.TenantId == tenantId)
            .Select(x => new EmployeeResponse
            {
                Id = x.Id,
                EmployeeCode = x.EmployeeCode,
                FirstName = x.FirstName,
                LastName = x.LastName,
                PhoneNumber = x.PhoneNumber,
                IsActive = x.IsActive,
                Username = x.User.Username
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<EmployeeResponse?> GetByUserIdAsync(
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await _db.Employees
            .AsNoTracking()
            .Where(x =>
                x.UserId == userId &&
                x.TenantId == tenantId)
            .Select(x => new EmployeeResponse
            {
                Id = x.Id,
                EmployeeCode = x.EmployeeCode,
                FirstName = x.FirstName,
                LastName = x.LastName,
                PhoneNumber = x.PhoneNumber,
                IsActive = x.IsActive,
                Username = x.User.Username
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
