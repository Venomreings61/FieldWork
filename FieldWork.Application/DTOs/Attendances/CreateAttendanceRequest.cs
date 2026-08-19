namespace FieldWork.Application.DTOs.Attendances;

public class CreateAttendanceRequest
{
    public string ClientAttendanceId { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public DateTimeOffset RecordedAt { get; set; }

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public decimal? AccuracyMeters { get; set; }

    public string Source { get; set; } = string.Empty;
}