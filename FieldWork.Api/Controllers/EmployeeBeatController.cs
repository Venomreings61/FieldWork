using FieldWork.Application.DTOs.EmployeeBeats;
using FieldWork.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FieldWork.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class EmployeeBeatController : ControllerBase
{
    private readonly IEmployeeBeatService _employeeBeatService;

    public EmployeeBeatController(IEmployeeBeatService employeeBeatService)
    {
        _employeeBeatService = employeeBeatService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateEmployeeBeatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _employeeBeatService.CreateAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var assignments = await _employeeBeatService.GetAllAsync(cancellationToken);
        return Ok(assignments);
    }
}