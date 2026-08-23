using FieldWork.Application.DTOs.Attendances;
using FieldWork.Application.DTOs.Common;
using FieldWork.Application.Exceptions;
using FieldWork.Application.Repositories;
using FieldWork.Application.Security;
using FieldWork.Domain.Enums;

namespace FieldWork.Application.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IEmployeeBeatRepository _employeeBeatRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IGeofenceService _geofenceService;
    private readonly IBeatRepository _beatRepository;

    public AttendanceService(
        IAttendanceRepository attendanceRepository,
        IEmployeeRepository employeeRepository,
        IEmployeeBeatRepository employeeBeatRepository,
        IBeatRepository beatRepository,
        ICurrentUser currentUser,
        IGeofenceService geofenceService)
    {
        _attendanceRepository = attendanceRepository;
        _employeeRepository = employeeRepository;
        _employeeBeatRepository = employeeBeatRepository;
        _beatRepository = beatRepository;
        _currentUser = currentUser;
        _geofenceService = geofenceService;
    }

    public async Task<AttendanceResponse> CreateAsync(
        CreateAttendanceRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var employee = await _employeeRepository.GetByUserIdAsync(
            _currentUser.UserId,
            _currentUser.TenantId,
            cancellationToken);

        if (employee is null)
        {
            throw new NotFoundException(
                "EMPLOYEE_NOT_FOUND",
                "Employee was not found in the current tenant.");
        }

        // Action/Source are now guaranteed valid enum values by model binding —
        // an invalid or missing value never reaches this point (rejected as 400 earlier).
        var action = request.Action!.Value;
        var source = request.Source!.Value;

        var existingAttendance =
            await _attendanceRepository.GetByClientAttendanceIdAsync(
                request.ClientAttendanceId,
                cancellationToken);

        if (existingAttendance is not null)
        {
            return existingAttendance;
        }

        var activeBeat =
            await _employeeBeatRepository.GetActiveByEmployeeAsync(
                employee.Id,
                cancellationToken);

        if (activeBeat is null)
        {
            throw new BusinessRuleException(
                "ATTENDANCE_NO_ACTIVE_BEAT",
                "Employee does not have an active beat assignment.");
        }

        var beat = await _beatRepository.GetByIdAsync(
            activeBeat.BeatId,
            _currentUser.TenantId,
            cancellationToken);

        if (beat is null)
        {
            throw new NotFoundException(
                "BEAT_NOT_FOUND",
                "Assigned beat was not found in the current tenant.");
        }

        var beatId = beat.Id;

        var isWithinGeofence = _geofenceService.IsWithinRadius(
            request.Latitude,
            request.Longitude,
            beat.CenterLatitude,
            beat.CenterLongitude,
            beat.RadiusMeters);

        if (!isWithinGeofence)
        {
            throw new BusinessRuleException(
                "ATTENDANCE_OUTSIDE_GEOFENCE",
                "Employee is outside the assigned beat geofence.");
        }

        var latestAction = await _attendanceRepository.GetLatestActionAsync(
            employee.Id,
            cancellationToken);

        if (latestAction is null && action == AttendanceAction.CHECK_OUT)
        {
            throw new ConflictException(
                "ATTENDANCE_CHECKOUT_WITHOUT_CHECKIN",
                "Employee must check in before checking out.");
        }

        if (latestAction == AttendanceAction.CHECK_IN && action == AttendanceAction.CHECK_IN)
        {
            throw new ConflictException(
                "ATTENDANCE_ALREADY_CHECKED_IN",
                "Employee is already checked in.");
        }

        if (latestAction == AttendanceAction.CHECK_OUT && action == AttendanceAction.CHECK_OUT)
        {
            throw new ConflictException(
                "ATTENDANCE_ALREADY_CHECKED_OUT",
                "Employee is already checked out.");
        }

        var receivedAt = DateTimeOffset.UtcNow;

        return await _attendanceRepository.CreateAsync(
            employee.Id,
            beatId,
            request,
            receivedAt,
            isWithinGeofence,
            cancellationToken);
    }

    public async Task<PagedResult<AttendanceResponse>> GetMyAttendanceAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var employee = await _employeeRepository.GetByUserIdAsync(
            _currentUser.UserId,
            _currentUser.TenantId,
            cancellationToken);

        if (employee is null)
        {
            throw new NotFoundException("EMPLOYEE_NOT_FOUND", "Employee was not found in the current tenant.");
        }

        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return await _attendanceRepository.GetByEmployeeAsync(
            employee.Id,
            page,
            pageSize,
            cancellationToken);
    }
}