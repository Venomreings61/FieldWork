using FieldWork.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FieldWork.Infrastructure.Data;

public class FieldWorkDbContext : DbContext
{
    public FieldWorkDbContext(DbContextOptions<FieldWorkDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<Beat> Beats => Set<Beat>();

    public DbSet<EmployeeBeat> EmployeeBeats => Set<EmployeeBeat>();

    public DbSet<Attendance> Attendances => Set<Attendance>();

    public DbSet<AttendanceSyncAttempt> AttendanceSyncAttempts => Set<AttendanceSyncAttempt>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(FieldWorkDbContext).Assembly);
    }
}