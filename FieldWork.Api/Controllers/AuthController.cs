
using FieldWork.Application.Authentication;
using FieldWork.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FieldWork.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUser _currentUser;

    public AuthController(IAuthService authService , ICurrentUser currentUser)
    {
        _authService = authService;
        _currentUser = currentUser;
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (result is null)
            return Unauthorized();

        return Ok(result);
    }

    
[HttpGet("me")]
[Authorize]
public IActionResult Me()
    {
        return Ok(new
        {
            UserId = _currentUser.UserId,
            TenantId = _currentUser.TenantId,
            Username = _currentUser.Username,
            Role = _currentUser.Role,
            IsAuthenticated = _currentUser.IsAuthenticated
        });
    }


}

