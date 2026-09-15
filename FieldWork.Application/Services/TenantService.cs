using FieldWork.Application.Repositories;
using FieldWork.Application.Security;

namespace FieldWork.Application.Services;

public class TenantService : ITenantService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ICurrentUser _currentUser;

    public TenantService(
        ITenantRepository tenantRepository,
        ICurrentUser currentUser)
    {
        _tenantRepository = tenantRepository;
        _currentUser = currentUser;
    }

    public async Task<bool> SetFaceVerificationRequiredAsync(
        bool required,
        CancellationToken cancellationToken = default)
    {
        return await _tenantRepository.SetFaceVerificationRequiredAsync(
            _currentUser.TenantId,
            required,
            cancellationToken);
    }
}