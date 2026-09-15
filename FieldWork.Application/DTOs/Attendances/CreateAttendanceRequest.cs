using System.ComponentModel.DataAnnotations;
using FieldWork.Domain.Enums;

namespace FieldWork.Application.DTOs.Attendances;

public class CreateAttendanceRequest
{
    [Required]
    public string ClientAttendanceId { get; set; } = string.Empty;

    [Required]
    public AttendanceAction? Action { get; set; }

    [Required]
    public DateTimeOffset? RecordedAt { get; set; }

    [Range(-90, 90)]
    public decimal Latitude { get; set; }

    [Range(-180, 180)]
    public decimal Longitude { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? AccuracyMeters { get; set; }

    [Required]
    public AttendanceSource? Source { get; set; }

    // Required only when the current tenant has FaceVerificationRequired = true.
    // Ignored entirely otherwise — no [Required] here, since that would break
    // every existing non-face-verification attendance request.
    public string? FaceImageBase64 { get; set; }
}