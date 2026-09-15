using FieldWork.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FieldWork.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class TenantController : ControllerBase
{
    private readonly ITenantService _tenantService;

    public TenantController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    public sealed record SetFaceVerificationRequest(bool Required);

    [HttpPut("face-verification")]
    public async Task<IActionResult> SetFaceVerification(
        [FromBody] SetFaceVerificationRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _tenantService.SetFaceVerificationRequiredAsync(
            request.Required,
            cancellationToken);

        if (!updated)
        {
            return NotFound(new
            {
                error = "TENANT_NOT_FOUND",
                message = "The authenticated tenant was not found."
            });
        }

        return Ok(new
        {
            faceVerificationRequired = request.Required
        });
    }
}