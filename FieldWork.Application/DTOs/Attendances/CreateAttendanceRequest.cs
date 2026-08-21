
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

    [Range(-90, 90)]
    public decimal Latitude { get; set; }

    [Range(-180, 180)]
    public decimal Longitude { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? AccuracyMeters { get; set; }

    [Required]
    public string Source { get; set; } = string.Empty;
}

