using FieldWork.Domain.Enums;

namespace FieldWork.Domain.Entities;

public class Attendance
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid? BeatId { get; set; }
    public string ClientAttendanceId { get; set; } = string.Empty;
    public AttendanceAction Action { get; set; }
    public DateTimeOffset RecordedAt { get; set; }
    public DateTimeOffset ReceivedAt { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public decimal? AccuracyMeters { get; set; }
    public AttendanceSource Source { get; set; }
    public SyncStatus SyncStatus { get; set; }
    public bool IsWithinGeofence { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}