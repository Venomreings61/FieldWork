
using FieldWork.Application.DTOs.EmployeeBeats;
using FieldWork.Application.Repositories;
using FieldWork.Application.Security;


namespace FieldWork.Application.Services;

public class EmployeeBeatService : IEmployeeBeatService
{
    private readonly IEmployeeBeatRepository _repository;
    private readonly ICurrentUser _currentUser;

    public EmployeeBeatService(
        IEmployeeBeatRepository repository,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<EmployeeBeatResponse> CreateAsync(
        CreateEmployeeBeatRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _currentUser.TenantId;

        // 1. Employee must belong to current tenant
        var employeeExists = await _repository.EmployeeExistsAsync(
            request.EmployeeId,
            tenantId,
            cancellationToken);

        if (!employeeExists)
        {
            throw new InvalidOperationException(
                "Employee was not found in the current tenant.");
        }

        // 2. Beat must belong to current tenant
        var beatExists = await _repository.BeatExistsAsync(
            request.BeatId,
            tenantId,
            cancellationToken);

        if (!beatExists)
        {
            throw new InvalidOperationException(
                "Beat was not found in the current tenant.");
        }

        // 3. Employee can only have one active beat
        var hasActiveAssignment = await _repository.HasActiveAssignmentAsync(
            request.EmployeeId,
            cancellationToken);

        if (hasActiveAssignment)
        {
            throw new InvalidOperationException(
                "Employee already has an active beat.");
        }

        // 4. All business rules passed
        return await _repository.CreateAsync(
            request,
            cancellationToken);
    }

    public async Task<IReadOnlyList<EmployeeBeatResponse>> GetAllAsync(
    CancellationToken cancellationToken = default)
    {
        return await _repository.GetAllAsync(
            _currentUser.TenantId,
            cancellationToken);
    }
}





