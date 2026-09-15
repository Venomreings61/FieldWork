using FieldWork.Domain.Entities;
using FieldWork.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using FieldWork.Application.Security;
using NetTopologySuite.Geometries;
using NetTopologySuite;

namespace FieldWork.Infrastructure.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(
        FieldWorkDbContext db,
        IPasswordHasher passwordHasher)
    {
        var now = DateTimeOffset.UtcNow;
        var defaultPasswordHash = passwordHasher.Hash("Password@123");

        var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
        Polygon CreateSquarePolygon(double minLng, double minLat, double maxLng, double maxLat)
        {
            var ring = geometryFactory.CreateLinearRing(new[]
            {
                new Coordinate(minLng, minLat),
                new Coordinate(maxLng, minLat),
                new Coordinate(maxLng, maxLat),
                new Coordinate(minLng, maxLat),
                new Coordinate(minLng, minLat)
            });
            return geometryFactory.CreatePolygon(ring);
        }

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
                TenantId = tenantA.Id,
                EmployeeCode = "EMP001",
                FirstName = "Demo",
                LastName = "Employee",
                PhoneNumber = "9999999999",
                IsActive = true,
                CreatedAt = now
            };
            db.Employees.Add(employeeA);
        }

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
                BoundaryPolygon = CreateSquarePolygon(72.8758, 19.0742, 72.8796, 19.0778),
                IsActive = true,
                CreatedAt = now
            };
            db.Beats.Add(beatA);
        }
        else
        {
            beatA.BoundaryPolygon ??= CreateSquarePolygon(72.8758, 19.0742, 72.8796, 19.0778);
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

        // --- Tenant A: Second Employee (Authorization Testing) ---
        var userA3 = await db.Users.FirstOrDefaultAsync(x => x.Username == "employee03");
        if (userA3 is null)
        {
            userA3 = new User
            {
                Id = Guid.NewGuid(),
                TenantId = tenantA.Id,
                Username = "employee03",
                Email = "employee03@fieldwork.local",
                PasswordHash = defaultPasswordHash,
                Role = "Employee",
                IsActive = true,
                CreatedAt = now
            };
            db.Users.Add(userA3);
        }
        else
        {
            userA3.TenantId = tenantA.Id;
            userA3.PasswordHash = defaultPasswordHash;
            userA3.IsActive = true;
        }

        var employeeA3 = await db.Employees.FirstOrDefaultAsync(x => x.UserId == userA3.Id);
        if (employeeA3 is null)
        {
            employeeA3 = new Employee
            {
                Id = Guid.NewGuid(),
                UserId = userA3.Id,
                TenantId = tenantA.Id,
                EmployeeCode = "EMP003",
                FirstName = "Second",
                LastName = "Employee",
                PhoneNumber = "7777777777",
                IsActive = true,
                CreatedAt = now
            };
            db.Employees.Add(employeeA3);
        }

        var adminUser = await db.Users.FirstOrDefaultAsync(x => x.Username == "admin");
        if (adminUser is null)
        {
            adminUser = new User
            {
                Id = Guid.NewGuid(),
                TenantId = tenantA.Id,
                Username = "admin",
                Email = "admin@fieldwork.local",
                PasswordHash = defaultPasswordHash,
                Role = "Admin",
                IsActive = true,
                CreatedAt = now
            };
            db.Users.Add(adminUser);
        }
        else
        {
            adminUser.TenantId = tenantA.Id;
            adminUser.PasswordHash = defaultPasswordHash;
            adminUser.Role = "Admin";
            adminUser.IsActive = true;
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
                TenantId = tenantB.Id,
                EmployeeCode = "EMP002",
                FirstName = "TenantB",
                LastName = "Employee",
                PhoneNumber = "8888888888",
                IsActive = true,
                CreatedAt = now
            };
            db.Employees.Add(employeeB);
        }

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
                BoundaryPolygon = CreateSquarePolygon(77.5928, 12.9698, 77.5964, 12.9734),
                IsActive = true,
                CreatedAt = now
            };
            db.Beats.Add(beatB);
        }
        else
        {
            beatB.BoundaryPolygon ??= CreateSquarePolygon(77.5928, 12.9698, 77.5964, 12.9734);
        }

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

        var adminUserB = await db.Users.FirstOrDefaultAsync(x => x.Username == "admin02");
        if (adminUserB is null)
        {
            adminUserB = new User
            {
                Id = Guid.NewGuid(),
                TenantId = tenantB.Id,
                Username = "admin02",
                Email = "admin02@tenantb.local",
                PasswordHash = defaultPasswordHash,
                Role = "Admin",
                IsActive = true,
                CreatedAt = now
            };
            db.Users.Add(adminUserB);
        }
        else
        {
            adminUserB.TenantId = tenantB.Id;
            adminUserB.PasswordHash = defaultPasswordHash;
            adminUserB.Role = "Admin";
            adminUserB.IsActive = true;
        }

        await db.SaveChangesAsync();
    }
}