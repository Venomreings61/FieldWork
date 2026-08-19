
using FieldWork.Application.DTOs.EmployeeBeats;
using FieldWork.Application.Repositories;
using FieldWork.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FieldWork.Infrastructure.Repositories;


public class EmployeeBeatRepository : IEmployeeBeatRepository
{
    private readonly FieldWorkDbContext _db;

    public EmployeeBeatRepository(FieldWorkDbContext db)
    {
        _db = db;
    }


public async Task<EmployeeBeatResponse> CreateAsync(
    Guid employeeId,
    Guid beatId,
    DateTimeOffset assignedFrom,
    CancellationToken cancellationToken = default)
    {
        var employeeBeat = new EmployeeBeat
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            BeatId = beatId,
            AssignedFrom = assignedFrom,
            AssignedTo = null,
            IsActive = true
        };

        _db.EmployeeBeats.Add(employeeBeat);

        await _db.SaveChangesAsync(cancellationToken);

        return new EmployeeBeatResponse
        {
            Id = employeeBeat.Id,
            EmployeeId = employeeBeat.EmployeeId,
            BeatId = employeeBeat.BeatId,
            AssignedFrom = employeeBeat.AssignedFrom,
            AssignedTo = employeeBeat.AssignedTo,
            IsActive = employeeBeat.IsActive
        };
    }

    public async Task<EmployeeBeatResponse?> GetActiveByEmployeeAsync(
    Guid employeeId,
    CancellationToken cancellationToken = default)
    {
        return await _db.EmployeeBeats
            .AsNoTracking()
            .Where(x =>
                x.EmployeeId == employeeId &&
                x.IsActive)
            .Select(x => new EmployeeBeatResponse
            {
                Id = x.Id,
                EmployeeId = x.EmployeeId,
                BeatId = x.BeatId,
                AssignedFrom = x.AssignedFrom,
                AssignedTo = x.AssignedTo,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);
    }


    public async Task<IReadOnlyList<EmployeeBeatResponse>> GetAllAsync(
    Guid tenantId,
    CancellationToken cancellationToken = default)
    {
        return await _db.EmployeeBeats
            .AsNoTracking()
            .Where(x =>
                x.Employee.User.TenantId == tenantId &&
                x.Beat.TenantId == tenantId)
            .OrderByDescending(x => x.AssignedFrom)
            .Select(x => new EmployeeBeatResponse
            {
                Id = x.Id,
                EmployeeId = x.EmployeeId,
                BeatId = x.BeatId,
                AssignedFrom = x.AssignedFrom,
                AssignedTo = x.AssignedTo,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }



    public async Task<bool> EmployeeExistsAsync(
        Guid employeeId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await _db.Employees
            .AnyAsync(
                x =>
                    x.Id == employeeId &&
                    x.User.TenantId == tenantId,
                cancellationToken);
    }

    public async Task<bool> BeatExistsAsync(
        Guid beatId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await _db.Beats
            .AnyAsync(
                x =>
                    x.Id == beatId &&
                    x.TenantId == tenantId,
                cancellationToken);
    }

    public async Task<bool> HasActiveAssignmentAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        return await _db.EmployeeBeats
            .AnyAsync(
                x =>
                    x.EmployeeId == employeeId &&
                    x.IsActive,
                cancellationToken);
    }
}

















