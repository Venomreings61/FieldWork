
using FieldWork.Application.DTOs.Beats;

namespace FieldWork.Application.Services;

public interface IBeatService
{
    Task<IReadOnlyList<BeatResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<BeatResponse?> GetByIdAsync(
        Guid beatId,
        CancellationToken cancellationToken = default);
}

