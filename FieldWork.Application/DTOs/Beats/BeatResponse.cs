
namespace FieldWork.Application.DTOs.Beats;

public class BeatResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public decimal CenterLatitude { get; set; }

    public decimal CenterLongitude { get; set; }

    public int RadiusMeters { get; set; }

    public bool IsActive { get; set; }
}

