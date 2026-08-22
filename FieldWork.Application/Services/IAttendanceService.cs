using FieldWork.Application.DTOs.Attendances;
using FieldWork.Application.DTOs.Common;

namespace FieldWork.Application.Services;

public interface IAttendanceService
{
    Task<AttendanceResponse> CreateAsync(
        CreateAttendanceRequest request,
        CancellationToken cancellationToken = default);

    Task<PagedResult<AttendanceResponse>> GetMyAttendanceAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}