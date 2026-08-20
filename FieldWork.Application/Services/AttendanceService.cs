using FieldWork.Application.DTOs.Attendances;
using FieldWork.Application.Repositories;
using FieldWork.Application.Security;

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
        // 1. User must be authenticated
        if (!_currentUser.IsAuthenticated)
        {
            throw new UnauthorizedAccessException(
                "User is not authenticated.");
        }

        // 2. Find employee from authenticated user
        var employee = await _employeeRepository.GetByUserIdAsync(
            _currentUser.UserId,
            _currentUser.TenantId,
            cancellationToken);

        if (employee is null)
        {
            throw new InvalidOperationException(
                "Employee was not found in the current tenant.");
        }

        // 3. Check whether this client request was already processed
        var existingAttendance =
            await _attendanceRepository.GetByClientAttendanceIdAsync(
                request.ClientAttendanceId,
                cancellationToken);

        if (existingAttendance is not null)
        {
            return existingAttendance;
        }

        // 4. Find employee's active beat assignment
        var activeBeat =
      await _employeeBeatRepository.GetActiveByEmployeeAsync(
          employee.Id,
          cancellationToken);

        Guid? beatId = null;
        var isWithinGeofence = false;

        if (activeBeat is not null)
        {
            var beat = await _beatRepository.GetByIdAsync(
                activeBeat.BeatId,
                _currentUser.TenantId,
                cancellationToken);

            if (beat is null)
            {
                throw new InvalidOperationException(
                    "Assigned beat was not found in the current tenant.");
            }

            beatId = beat.Id;

            isWithinGeofence = _geofenceService.IsWithinRadius(
                request.Latitude,
                request.Longitude,
                beat.CenterLatitude,
                beat.CenterLongitude,
                beat.RadiusMeters);
        }

        var latestAction = await _attendanceRepository.GetLatestActionAsync(
    employee.Id,
    cancellationToken);

        Console.WriteLine($"Latest attendance action: {latestAction}");

        if (latestAction is null &&
            request.Action == "CHECK_OUT")
        {
            throw new InvalidOperationException(
                "Employee must check in before checking out.");
        }

        if (latestAction == "CHECK_IN" &&
            request.Action == "CHECK_IN")
        {
            throw new InvalidOperationException(
                "Employee is already checked in.");
        }

        if (latestAction == "CHECK_OUT" &&
            request.Action == "CHECK_OUT")
        {
            throw new InvalidOperationException(
                "Employee is already checked out.");
        }

        // 5. Server-controlled timestamps
        var receivedAt = DateTimeOffset.UtcNow;
        

        // 6. Persist attendance
        return await _attendanceRepository.CreateAsync(
            employee.Id,
            beatId,
            request,
            receivedAt,
            isWithinGeofence,
            cancellationToken);
    }

    public async Task<IReadOnlyList<AttendanceResponse>> GetMyAttendanceAsync(
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            throw new UnauthorizedAccessException(
                "User is not authenticated.");
        }

        var employee = await _employeeRepository.GetByUserIdAsync(
            _currentUser.UserId,
            _currentUser.TenantId,
            cancellationToken);

        if (employee is null)
        {
            throw new InvalidOperationException(
                "Employee was not found in the current tenant.");
        }

        return await _attendanceRepository.GetByEmployeeAsync(
            employee.Id,
            cancellationToken);
    }
}