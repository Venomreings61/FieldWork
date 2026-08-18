
namespace FieldWork.Application.Security;

public interface ICurrentUser
{
    Guid UserId { get; }

    Guid TenantId { get; }

    string Username { get; }

    string Role { get; }

    bool IsAuthenticated { get; }
}

