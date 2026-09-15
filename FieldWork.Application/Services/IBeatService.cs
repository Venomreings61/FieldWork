
//using FieldWork.Application.DTOs.Beats;

using FieldWork.Application.DTOs.Beats;

namespace FieldWork.Application.Services;

public interface IBeatService
{
    Task<IReadOnlyList<BeatResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<BeatResponse?> GetByIdAsync(
        Guid beatId,
        CancellationToken cancellationToken = default);

    // Interface
    Task<BeatResponse> CreateAsync(CreateBeatRequest request, CancellationToken cancellationToken = default);
    Task<BeatResponse> UpdateAsync(Guid beatId, UpdateBeatRequest request, CancellationToken cancellationToken = default);

    Task<BeatResponse> CreateFromKmlAsync(
    string code,
    string? name,
    Stream kmlStream,
    CancellationToken cancellationToken = default);
}

