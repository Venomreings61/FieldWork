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
     CreateAttendanceRequest request,
     CancellationToken cancellationToken)
    {
        try
        {
            var result = await _attendanceService.CreateAsync(
                request,
                cancellationToken);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
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