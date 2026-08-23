
using FieldWork.Application.DTOs.Attendances;
using FieldWork.Application.DTOs.Common;
using FieldWork.Domain.Enums;

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

    Task<PagedResult<AttendanceResponse>> GetByEmployeeAsync(
     Guid employeeId,
     int page,
     int pageSize,
     CancellationToken cancellationToken = default);

    Task<AttendanceAction?> GetLatestActionAsync(
    Guid employeeId,
    CancellationToken cancellationToken = default);
}