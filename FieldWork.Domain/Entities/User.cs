namespace FieldWork.Domain.Entities;

public class User
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTimeOffset? LastLoginAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}