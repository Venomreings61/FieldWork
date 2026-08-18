
namespace FieldWork.Application.DTOs.Employees;

public class EmployeeResponse
{
    public Guid Id { get; set; }

    public string EmployeeCode { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string? LastName { get; set; }

    public string? PhoneNumber { get; set; }

    public bool IsActive { get; set; }

    public string Username { get; set; } = string.Empty;
}

