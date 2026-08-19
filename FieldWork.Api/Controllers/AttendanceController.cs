using FieldWork.Application.DTOs.Attendances;
using FieldWork.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FieldWork.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpPost]
    public async Task<ActionResult<AttendanceResponse>> Create(
        [FromBody] CreateAttendanceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _attendanceService.CreateAsync(
            request,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AttendanceResponse>>> GetMyAttendance(
        CancellationToken cancellationToken)
    {
        var result = await _attendanceService.GetMyAttendanceAsync(
            cancellationToken);

        return Ok(result);
    }
}