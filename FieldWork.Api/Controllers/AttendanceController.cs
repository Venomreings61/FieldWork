using FieldWork.Application.DTOs.Attendances;
using FieldWork.Application.DTOs.Common;
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
        var result = await _attendanceService.CreateAsync(request, cancellationToken);
        return Ok(result);
    }


    [HttpGet]
    public async Task<ActionResult<PagedResult<AttendanceResponse>>> GetMyAttendance(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    CancellationToken cancellationToken = default)
    {
        var result = await _attendanceService.GetMyAttendanceAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }


}
