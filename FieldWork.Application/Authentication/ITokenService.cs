
namespace FieldWork.Application.Authentication;

public interface ITokenService
{
    (string AccessToken, DateTimeOffset ExpiresAt) GenerateToken(
        Guid userId,
        Guid tenantId,
        string username,
        string role);
}

