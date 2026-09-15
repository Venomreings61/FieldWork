namespace FieldWork.Application.Repositories;

public interface ITenantRepository
{
    Task<bool> IsFaceVerificationRequiredAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<bool> SetFaceVerificationRequiredAsync(
        Guid tenantId,
        bool required,
        CancellationToken cancellationToken = default);
}