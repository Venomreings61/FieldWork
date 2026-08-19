using FieldWork.Application.DTOs.Attendances;

namespace FieldWork.Application.Services;

public interface IAttendanceService
{
    Task<AttendanceResponse> CreateAsync(
        CreateAttendanceRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AttendanceResponse>> GetMyAttendanceAsync(
        CancellationToken cancellationToken = default);
}