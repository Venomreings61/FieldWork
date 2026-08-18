
using FieldWork.Domain.Entities;

public class EmployeeBeat
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }

    public Employee Employee { get; set; } = null!;

    public Guid BeatId { get; set; }

    public Beat Beat { get; set; } = null!;

    public DateTimeOffset AssignedFrom { get; set; }

    public DateTimeOffset? AssignedTo { get; set; }

    public bool IsActive { get; set; }
}

