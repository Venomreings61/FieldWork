using FieldWork.Application.DTOs.EmployeeBeats;
using FieldWork.Application.Exceptions;
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

        var employeeExists = await _repository.EmployeeExistsAsync(
            request.EmployeeId,
            tenantId,
            cancellationToken);

        if (!employeeExists)
        {
            throw new NotFoundException("EMPLOYEE_NOT_FOUND", "Employee was not found in the current tenant.");
        }

        var beatExists = await _repository.BeatExistsAsync(
            request.BeatId,
            tenantId,
            cancellationToken);

        if (!beatExists)
        {
            throw new NotFoundException("BEAT_NOT_FOUND", "Beat was not found in the current tenant.");
        }

        var hasActiveAssignment =
            await _repository.HasActiveAssignmentAsync(
                request.EmployeeId,
                cancellationToken);

        if (hasActiveAssignment)
        {
            throw new ConflictException("EMPLOYEE_ACTIVE_BEAT_EXISTS", "Employee already has an active beat.");
        }

        var assignedFrom = DateTimeOffset.UtcNow;

        return await _repository.CreateAsync(
            request.EmployeeId,
            request.BeatId,
            assignedFrom,
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