namespace FieldWork.Application.Configuration;

public class FaceVerificationOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;

    // Server-owned. Never accepted from the client for attendance.
    public double AttendanceThreshold { get; set; } = 0.40;
}