using FieldWork.Application.DTOs.Attendances;
using FieldWork.Application.Repositories;
using FieldWork.Domain.Entities;
using FieldWork.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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
            Action = request.Action,
            RecordedAt = request.RecordedAt,
            ReceivedAt = receivedAt,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            AccuracyMeters = request.AccuracyMeters,
            Source = request.Source,
            SyncStatus = "Synced",
            IsWithinGeofence = isWithinGeofence,
            CreatedAt = receivedAt
        };

        _db.Attendances.Add(attendance);

        await _db.SaveChangesAsync(cancellationToken);

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

    public async Task<IReadOnlyList<AttendanceResponse>> GetByEmployeeAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        return await _db.Attendances
            .AsNoTracking()
            .Where(x => x.EmployeeId == employeeId)
            .OrderByDescending(x => x.RecordedAt)
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
    }


    public async Task<string?> GetLatestActionAsync(
     Guid employeeId,
     CancellationToken cancellationToken = default)
    {
        return await _db.Attendances
            .AsNoTracking()
            .Where(x => x.EmployeeId == employeeId)
            .OrderByDescending(x => x.ReceivedAt)
            .Select(x => x.Action)
            .FirstOrDefaultAsync(cancellationToken);
    }
}



