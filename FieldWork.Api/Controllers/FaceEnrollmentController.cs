using FieldWork.Application.DTOs;   // not FieldWork.Application.DTOs.Face
using FieldWork.Application.DTOs.Face;
using FieldWork.Application.Exceptions;
using FieldWork.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FieldWork.Api.Controllers;

[ApiController]
[Route("api/v1/employees/{employeeId:guid}/face")]
[Authorize(Roles = "Admin")]
public class FaceEnrollmentController : ControllerBase
{
    private readonly IFaceEnrollmentService _faceEnrollmentService;

    public FaceEnrollmentController(IFaceEnrollmentService faceEnrollmentService)
    {
        _faceEnrollmentService = faceEnrollmentService;
    }

    [HttpPost("enroll")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(EnrollFaceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnrollFace(
        [FromRoute] Guid employeeId,
        IFormFile image,
        CancellationToken cancellationToken)
    {
        if (image is null || image.Length == 0)
        {
            throw new BusinessRuleException("INVALID_IMAGE", "An image file is required.");
        }

        using var memoryStream = new MemoryStream();
        await image.CopyToAsync(memoryStream, cancellationToken);
        var imageBytes = memoryStream.ToArray();

        var request = new EnrollFaceRequest(employeeId, imageBytes);
        var result = await _faceEnrollmentService.EnrollFaceAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFace([FromRoute] Guid employeeId, CancellationToken cancellationToken)
    {
        var deleted = await _faceEnrollmentService.DeleteFaceEnrollmentAsync(employeeId, cancellationToken);
        if (!deleted)
        {
            throw new NotFoundException(
                "FACE_ENROLLMENT_NOT_FOUND",
                $"No face enrollment found for employee '{employeeId}'.");
        }

        return NoContent();
    }
}