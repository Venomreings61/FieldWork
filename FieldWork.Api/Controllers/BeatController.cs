using FieldWork.Application.DTOs.Beats;
using FieldWork.Application.Exceptions;
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
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var beats = await _beatService.GetAllAsync(cancellationToken);
        return Ok(beats);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var beat = await _beatService.GetByIdAsync(id, cancellationToken);

        if (beat is null)
        {
            throw new NotFoundException("BEAT_NOT_FOUND", "Beat not found.");
        }

        return Ok(beat);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
    [FromBody] CreateBeatRequest request,
    CancellationToken cancellationToken)
    {
        var result = await _beatService.CreateAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateBeatRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _beatService.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    // BeatController.cs
    [HttpPost("import-kml")]
    public async Task<IActionResult> ImportKml(
        [FromForm] string code,
        [FromForm] string? name,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            throw new BusinessRuleException("KML_FILE_REQUIRED", "A KML file is required.");
        }

        await using var stream = file.OpenReadStream();
        var result = await _beatService.CreateFromKmlAsync(code, name, stream, cancellationToken);
        return Ok(result);
    }
}