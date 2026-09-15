
//using FieldWork.Application.DTOs.Beats;

using NetTopologySuite.Geometries;

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

    Task<BeatResponse> CreateAsync(
    Guid tenantId,
    string code,
    string name,
    Polygon boundaryPolygon,
    CancellationToken cancellationToken = default);

    Task<BeatResponse?> UpdateAsync(
        Guid beatId,
        Guid tenantId,
        string name,
        Polygon boundaryPolygon,
        CancellationToken cancellationToken = default);


}

