
using FieldWork.Application.DTOs.Beats;

namespace FieldWork.Application.Repositories;

public interface IBeatRepository
{
    Task<IReadOnlyList<BeatResponse>> GetAllAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task<BeatResponse?> GetByIdAsync(
        Guid beatId,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    
}

