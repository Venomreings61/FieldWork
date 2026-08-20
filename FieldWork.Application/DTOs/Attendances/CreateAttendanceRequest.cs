
using System.ComponentModel.DataAnnotations;

namespace FieldWork.Application.DTOs.Attendances;

public class CreateAttendanceRequest
{
    [Required]
    public string ClientAttendanceId { get; set; } = string.Empty;

    [Required]
    public string Action { get; set; } = string.Empty;

    [Required]
    public DateTimeOffset? RecordedAt { get; set; }

    [Required]
    public decimal Latitude { get; set; }

    [Required]
    public decimal Longitude { get; set; }

    public decimal? AccuracyMeters { get; set; }

    [Required]
    public string Source { get; set; } = string.Empty;
}