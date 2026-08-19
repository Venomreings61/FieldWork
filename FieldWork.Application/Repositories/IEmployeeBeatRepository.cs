
using FieldWork.Application.DTOs.EmployeeBeats;


namespace FieldWork.Application.Repositories; 
public interface IEmployeeBeatRepository
{ 

Task<EmployeeBeatResponse> CreateAsync(
    Guid employeeId,
    Guid beatId,
    DateTimeOffset assignedFrom,
    CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EmployeeBeatResponse>> GetAllAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<bool> EmployeeExistsAsync(
        Guid employeeId,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<bool> BeatExistsAsync(
        Guid beatId,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<bool> HasActiveAssignmentAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default);


}

