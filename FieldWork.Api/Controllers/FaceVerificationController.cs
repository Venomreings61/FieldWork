using FieldWork.Application.DTOs;
using FieldWork.Application.DTOs.Face;
using FieldWork.Application.Exceptions;
using FieldWork.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FieldWork.Api.Controllers;

[ApiController]
[Route("api/v1/employees/{employeeId:guid}/face")]
[Authorize]
public class FaceVerificationController : ControllerBase
{
    private readonly IFaceEmployeeVerificationService _service;

    public FaceVerificationController(IFaceEmployeeVerificationService service)
    {
        _service = service;
    }

    [HttpPost("verify")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(VerifyFaceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Verify(
        [FromRoute] Guid employeeId,
        IFormFile image,
        [FromForm] double? threshold,
        CancellationToken cancellationToken)
    {
        if (image is null || image.Length == 0)
        {
            throw new BusinessRuleException("INVALID_IMAGE", "An image file is required.");
        }

        using var memoryStream = new MemoryStream();
        await image.CopyToAsync(memoryStream, cancellationToken);
        var imageBytes = memoryStream.ToArray();

        var request = new VerifyFaceRequest(employeeId, imageBytes, threshold);
        var result = await _service.VerifyFaceAsync(request, cancellationToken);
        return Ok(result);
    }
}