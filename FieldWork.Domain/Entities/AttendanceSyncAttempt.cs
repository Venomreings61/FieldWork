namespace FieldWork.Domain.Entities;

public class AttendanceSyncAttempt
{
    public Guid Id { get; set; }

    public Guid AttendanceId { get; set; }

    public DateTimeOffset AttemptedAt { get; set; }

    public bool IsSuccessful { get; set; }

    public string? ErrorMessage { get; set; }
}