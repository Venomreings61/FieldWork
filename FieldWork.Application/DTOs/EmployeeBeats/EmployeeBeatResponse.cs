
namespace FieldWork.Application.DTOs.EmployeeBeats;

public class EmployeeBeatResponse
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }

    public Guid BeatId { get; set; }

    public DateTimeOffset AssignedFrom { get; set; }

    public DateTimeOffset? AssignedTo { get; set; }

    public bool IsActive { get; set; }
}

