using FieldWork.Application.Security;
using Microsoft.Extensions.Logging;

namespace FieldWork.Application.Authentication;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);

        if (user is null)
        {
            _logger.LogWarning("Login attempt for unknown username {Username}.", request.Username);
            return null;
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt for inactive user {Username} (UserId: {UserId}).", request.Username, user.Id);
            return null;
        }

        var passwordValid = _passwordHasher.Verify(request.Password, user.PasswordHash);

        if (!passwordValid)
        {
            _logger.LogWarning("Invalid password for user {Username} (UserId: {UserId}).", request.Username, user.Id);
            return null;
        }

        var (accessToken, expiresAt) = _tokenService.GenerateToken(user.Id, user.TenantId, user.Username, user.Role);

        _logger.LogInformation(
            "User {Username} logged in successfully. UserId: {UserId}, TenantId: {TenantId}.",
            user.Username, user.Id, user.TenantId);

        return new LoginResponse
        {
            AccessToken = accessToken,
            ExpiresAt = expiresAt,
            UserId = user.Id,
            Username = user.Username,
            Role = user.Role
        };
    }
}