
using FieldWork.Application.DTOs.Beats;
using FieldWork.Application.Repositories;
using FieldWork.Application.Security;

namespace FieldWork.Application.Services;

public class BeatService : IBeatService
{
    private readonly IBeatRepository _repository;
    private readonly ICurrentUser _currentUser;

    public BeatService(
        IBeatRepository repository,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<BeatResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetAllAsync(
            _currentUser.TenantId,
            cancellationToken);
    }

    public async Task<BeatResponse?> GetByIdAsync(
        Guid beatId,
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(
            beatId,
            _currentUser.TenantId,
            cancellationToken);
    }
}

