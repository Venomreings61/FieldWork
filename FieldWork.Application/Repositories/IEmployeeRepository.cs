
using FieldWork.Application.DTOs.Employees;

namespace FieldWork.Application.Repositories;

public interface IEmployeeRepository
{
    Task<IReadOnlyList<EmployeeResponse>> GetAllAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<EmployeeResponse?> GetByIdAsync(
        Guid employeeId,
        Guid tenantId,
        CancellationToken cancellationToken = default);
}

