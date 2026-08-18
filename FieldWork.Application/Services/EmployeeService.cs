
using FieldWork.Application.DTOs.Employees;
using FieldWork.Application.Repositories;
using FieldWork.Application.Security;

namespace FieldWork.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repository;
    private readonly ICurrentUser _currentUser;

    public EmployeeService(
        IEmployeeRepository repository,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<EmployeeResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetAllAsync(
            _currentUser.TenantId,
            cancellationToken);
    }

    public async Task<EmployeeResponse?> GetByIdAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(
            employeeId,
            _currentUser.TenantId,
            cancellationToken);
    }
}

