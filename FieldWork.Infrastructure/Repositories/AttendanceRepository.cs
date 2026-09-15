using FieldWork.Application.DTOs.Attendances;
using FieldWork.Application.DTOs.Common;
using FieldWork.Application.Repositories;
using FieldWork.Domain.Entities;
using FieldWork.Domain.Enums;
using FieldWork.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FieldWork.Infrastructure.Repositories;

public class AttendanceRepository : IAttendanceRepository
{
    private readonly FieldWorkDbContext _db;

    public AttendanceRepository(FieldWorkDbContext db)
    {
        _db = db;
    }

    public async Task<AttendanceResponse?> GetByClientAttendanceIdAsync(
        string clientAttendanceId,
        CancellationToken cancellationToken = default)
    {
        return await _db.Attendances
            .AsNoTracking()
            .Where(x => x.ClientAttendanceId == clientAttendanceId)
            .Select(x => new AttendanceResponse
            {
                Id = x.Id,
                EmployeeId = x.EmployeeId,
                BeatId = x.BeatId,
                ClientAttendanceId = x.ClientAttendanceId,
                Action = x.Action,
                RecordedAt = x.RecordedAt,
                ReceivedAt = x.ReceivedAt,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                AccuracyMeters = x.AccuracyMeters,
                Source = x.Source,
                SyncStatus = x.SyncStatus,
                IsWithinGeofence = x.IsWithinGeofence,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AttendanceResponse> CreateAsync(
        Guid employeeId,
        Guid? beatId,
        CreateAttendanceRequest request,
        DateTimeOffset receivedAt,
        bool isWithinGeofence,
        CancellationToken cancellationToken = default)
    {
        var attendance = new Attendance
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            BeatId = beatId,
            ClientAttendanceId = request.ClientAttendanceId,
            Action = request.Action!.Value,
            RecordedAt = request.RecordedAt!.Value,
            ReceivedAt = receivedAt,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            AccuracyMeters = request.AccuracyMeters,
            Source = request.Source!.Value,
            SyncStatus = SyncStatus.Synced,
            IsWithinGeofence = isWithinGeofence,
            CreatedAt = receivedAt
        };

        _db.Attendances.Add(attendance);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            // Another request won the race and inserted this ClientAttendanceId first.
            // Detach the failed entity and return the existing record instead.
            _db.Entry(attendance).State = EntityState.Detached;

            var existing = await GetByClientAttendanceIdAsync(
                request.ClientAttendanceId,
                cancellationToken);

            if (existing is not null)
            {
                return existing;
            }

            throw; // truly unexpected — rethrow if somehow still not found
        }

        return new AttendanceResponse
        {
            Id = attendance.Id,
            EmployeeId = attendance.EmployeeId,
            BeatId = attendance.BeatId,
            ClientAttendanceId = attendance.ClientAttendanceId,
            Action = attendance.Action,
            RecordedAt = attendance.RecordedAt,
            ReceivedAt = attendance.ReceivedAt,
            Latitude = attendance.Latitude,
            Longitude = attendance.Longitude,
            AccuracyMeters = attendance.AccuracyMeters,
            Source = attendance.Source,
            SyncStatus = attendance.SyncStatus,
            IsWithinGeofence = attendance.IsWithinGeofence,
            CreatedAt = attendance.CreatedAt
        };
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException is Npgsql.PostgresException pgEx &&
               pgEx.SqlState == "23505"; // Postgres unique_violation error code
    }

    public async Task<PagedResult<AttendanceResponse>> GetByEmployeeAsync(
        Guid employeeId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Attendances
            .AsNoTracking()
            .Where(x => x.EmployeeId == employeeId)
            .OrderByDescending(x => x.ReceivedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AttendanceResponse
            {
                Id = x.Id,
                EmployeeId = x.EmployeeId,
                BeatId = x.BeatId,
                ClientAttendanceId = x.ClientAttendanceId,
                Action = x.Action,
                RecordedAt = x.RecordedAt,
                ReceivedAt = x.ReceivedAt,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                AccuracyMeters = x.AccuracyMeters,
                Source = x.Source,
                SyncStatus = x.SyncStatus,
                IsWithinGeofence = x.IsWithinGeofence,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<AttendanceResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<AttendanceAction?> GetLatestActionAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        return await _db.Attendances
            .AsNoTracking()
            .Where(x => x.EmployeeId == employeeId)
            .OrderByDescending(x => x.ReceivedAt)
            .Select(x => (AttendanceAction?)x.Action)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        return await _db.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task AcquireEmployeeLockAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        // Convert the first 8 bytes of the Guid to a 64-bit integer (bigint).
        // pg_advisory_xact_lock automatically releases when the transaction finishes (commit or rollback).
        long lockKey = BitConverter.ToInt64(employeeId.ToByteArray(), 0);

        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock({lockKey})",
            cancellationToken);
    }
}