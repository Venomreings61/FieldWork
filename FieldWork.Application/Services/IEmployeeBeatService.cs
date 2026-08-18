
using FieldWork.Application.DTOs.EmployeeBeats;

namespace FieldWork.Application.Services;

public interface IEmployeeBeatService
{
    Task<EmployeeBeatResponse> CreateAsync(
        CreateEmployeeBeatRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EmployeeBeatResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);
}

