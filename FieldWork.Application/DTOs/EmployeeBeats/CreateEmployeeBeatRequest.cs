
namespace FieldWork.Application.DTOs.EmployeeBeats;

public class CreateEmployeeBeatRequest
{
    public Guid EmployeeId { get; set; }

    public Guid BeatId { get; set; }

    public DateTimeOffset AssignedFrom { get; set; }
}

