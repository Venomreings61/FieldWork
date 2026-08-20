

using FieldWork.Application.DTOs.Attendances;

namespace FieldWork.Application.Repositories;

public interface IAttendanceRepository
{
    Task<AttendanceResponse?> GetByClientAttendanceIdAsync(
        string clientAttendanceId,
        CancellationToken cancellationToken = default);

    Task<AttendanceResponse> CreateAsync(
        Guid employeeId,
        Guid? beatId,
        CreateAttendanceRequest request,
        DateTimeOffset receivedAt,
        bool isWithinGeofence,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AttendanceResponse>> GetByEmployeeAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default);

    Task<string?> GetLatestActionAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default);
}