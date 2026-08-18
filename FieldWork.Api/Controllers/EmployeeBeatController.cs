
using FieldWork.Application.DTOs.EmployeeBeats;
using FieldWork.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FieldWork.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeeBeatController : ControllerBase
{
    private readonly IEmployeeBeatService _employeeBeatService;

    public EmployeeBeatController(
        IEmployeeBeatService employeeBeatService)
    {
        _employeeBeatService = employeeBeatService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateEmployeeBeatRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _employeeBeatService
                .CreateAsync(request, cancellationToken);

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
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var assignments = await _employeeBeatService
            .GetAllAsync(cancellationToken);

        return Ok(assignments);
    }
}

