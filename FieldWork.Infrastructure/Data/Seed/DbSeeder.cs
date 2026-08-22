using FieldWork.Domain.Entities;
using FieldWork.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using FieldWork.Application.Security;

namespace FieldWork.Infrastructure.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(
        FieldWorkDbContext db,
        IPasswordHasher passwordHasher)
    {
        var now = DateTimeOffset.UtcNow;
        var defaultPasswordHash = passwordHasher.Hash("Password@123");

        // ============================================================
        // TENANT A - DEMO DATA
        // ============================================================
        var tenantA = await db.Tenants.FirstOrDefaultAsync(x => x.Code == "FIELDWORK");
        if (tenantA is null)
        {
            tenantA = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = "FieldWork Demo",
                Code = "FIELDWORK",
                IsActive = true,
                CreatedAt = now
            };
            db.Tenants.Add(tenantA);
        }

        var userA = await db.Users.FirstOrDefaultAsync(x => x.Username == "employee01");
        if (userA is null)
        {
            userA = new User
            {
                Id = Guid.NewGuid(),
                TenantId = tenantA.Id,
                Username = "employee01",
                Email = "employee01@fieldwork.local",
                PasswordHash = defaultPasswordHash,
                Role = "Employee",
                IsActive = true,
                CreatedAt = now
            };
            db.Users.Add(userA);
        }
        else
        {
            userA.TenantId = tenantA.Id;
            userA.PasswordHash = defaultPasswordHash;
            userA.IsActive = true;
        }

        var employeeA = await db.Employees.FirstOrDefaultAsync(x => x.UserId == userA.Id);
        if (employeeA is null)
        {
            employeeA = new Employee
            {
                Id = Guid.NewGuid(),
                UserId = userA.Id,
                EmployeeCode = "EMP001",
                FirstName = "Demo",
                LastName = "Employee",
                PhoneNumber = "9999999999",
                IsActive = true,
                CreatedAt = now
            };
            db.Employees.Add(employeeA);
        }

        // FIX 1: scope by TenantId + Code, not Code alone
        var beatA = await db.Beats
            .FirstOrDefaultAsync(x => x.TenantId == tenantA.Id && x.Code == "BEAT001");
        if (beatA is null)
        {
            beatA = new Beat
            {
                Id = Guid.NewGuid(),
                TenantId = tenantA.Id,
                Name = "Demo Beat",
                Code = "BEAT001",
                CenterLatitude = 19.0760m,
                CenterLongitude = 72.8777m,
                RadiusMeters = 500,
                IsActive = true,
                CreatedAt = now
            };
            db.Beats.Add(beatA);
        }

        var employeeBeatA = await db.EmployeeBeats
            .FirstOrDefaultAsync(x => x.EmployeeId == employeeA.Id && x.BeatId == beatA.Id);

        if (employeeBeatA is null)
        {
            db.EmployeeBeats.Add(new EmployeeBeat
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeA.Id,
                BeatId = beatA.Id,
                AssignedFrom = now,
                AssignedTo = null,
                IsActive = true
            });
        }

        await db.SaveChangesAsync();

        // ============================================================
        // TENANT B - DEVELOPMENT TEST DATA
        // ============================================================
        var tenantB = await db.Tenants.FirstOrDefaultAsync(x => x.Code == "TENANT-B");
        if (tenantB is null)
        {
            tenantB = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = "Demo Tenant B",
                Code = "TENANT-B",
                IsActive = true,
                CreatedAt = now
            };
            db.Tenants.Add(tenantB);
        }

        var userB = await db.Users.FirstOrDefaultAsync(x => x.Username == "employee02");
        if (userB is null)
        {
            userB = new User
            {
                Id = Guid.NewGuid(),
                TenantId = tenantB.Id,
                Username = "employee02",
                Email = "employee02@tenantb.local",
                PasswordHash = defaultPasswordHash,
                Role = "Employee",
                IsActive = true,
                CreatedAt = now
            };
            db.Users.Add(userB);
        }
        else
        {
            userB.TenantId = tenantB.Id;
            userB.PasswordHash = defaultPasswordHash;
            userB.IsActive = true;
        }

        var employeeB = await db.Employees.FirstOrDefaultAsync(x => x.UserId == userB.Id);
        if (employeeB is null)
        {
            employeeB = new Employee
            {
                Id = Guid.NewGuid(),
                UserId = userB.Id,
                EmployeeCode = "EMP002",
                FirstName = "TenantB",
                LastName = "Employee",
                PhoneNumber = "8888888888",
                IsActive = true,
                CreatedAt = now
            };
            db.Employees.Add(employeeB);
        }

        // FIX 1: scope by TenantId + Code, not Code alone
        var beatB = await db.Beats
            .FirstOrDefaultAsync(x => x.TenantId == tenantB.Id && x.Code == "BEAT002");
        if (beatB is null)
        {
            beatB = new Beat
            {
                Id = Guid.NewGuid(),
                TenantId = tenantB.Id,
                Name = "Tenant B Beat",
                Code = "BEAT002",
                CenterLatitude = 12.9716m,
                CenterLongitude = 77.5946m,
                RadiusMeters = 500,
                IsActive = true,
                CreatedAt = now
            };
            db.Beats.Add(beatB);
        }

        // FIX 2: assign employeeB to beatB, mirroring Tenant A's structure
        // ↓↓↓ ADD THIS NEW BLOCK ↓↓↓
        var employeeBeatB = await db.EmployeeBeats
            .FirstOrDefaultAsync(x =>
                x.EmployeeId == employeeB.Id &&
                x.BeatId == beatB.Id);

        if (employeeBeatB is null)
        {
            db.EmployeeBeats.Add(new EmployeeBeat
            {
                Id = Guid.NewGuid(),
                EmployeeId = employeeB.Id,
                BeatId = beatB.Id,
                AssignedFrom = now,
                AssignedTo = null,
                IsActive = true
            });
        }
        // ↑↑↑ ADD THIS NEW BLOCK ↑↑↑



        await db.SaveChangesAsync();
    }
}