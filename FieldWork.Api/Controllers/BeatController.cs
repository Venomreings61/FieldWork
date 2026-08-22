
using FieldWork.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FieldWork.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class BeatController : ControllerBase
{
    private readonly IBeatService _beatService;

    public BeatController(IBeatService beatService)
    {
        _beatService = beatService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var beats = await _beatService
            .GetAllAsync(cancellationToken);

        return Ok(beats);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var beat = await _beatService
            .GetByIdAsync(id, cancellationToken);

        if (beat is null)
        {
            return NotFound(new
            {
                message = "Beat not found."
            });
        }

        return Ok(beat);
    }
}

