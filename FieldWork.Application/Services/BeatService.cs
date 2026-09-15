
//using FieldWork.Application.DTOs.Beats;
using FieldWork.Application.DTOs.Beats;
using FieldWork.Application.Exceptions;
using FieldWork.Application.Repositories;
using FieldWork.Application.Security;

namespace FieldWork.Application.Services;

public class BeatService : IBeatService
{
    private readonly IBeatRepository _repository;
    private readonly ICurrentUser _currentUser;
    private readonly IBeatKmlImporter _kmlImporter;

    public BeatService(
        IBeatRepository repository,
        ICurrentUser currentUser,
        IBeatKmlImporter kmlImporter)
    {
        _repository = repository;
        _currentUser = currentUser;
        _kmlImporter = kmlImporter;
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

    // Implementation
    public async Task<BeatResponse> CreateAsync(
        CreateBeatRequest request,
        CancellationToken cancellationToken = default)
    {
        var polygon = BeatGeometryValidator.BuildAndValidate(request.BoundaryPoints);

        return await _repository.CreateAsync(
            _currentUser.TenantId,
            request.Code,
            request.Name,
            polygon,
            cancellationToken);
    }

    public async Task<BeatResponse> UpdateAsync(
        Guid beatId,
        UpdateBeatRequest request,
        CancellationToken cancellationToken = default)
    {
        var polygon = BeatGeometryValidator.BuildAndValidate(request.BoundaryPoints);

        var result = await _repository.UpdateAsync(
            beatId,
            _currentUser.TenantId,
            request.Name,
            polygon,
            cancellationToken);

        if (result is null)
        {
            throw new NotFoundException("BEAT_NOT_FOUND", "Beat not found in the current tenant.");
        }

        return result;
    }

    public async Task<BeatResponse> CreateFromKmlAsync(
    string code,
    string? name,
    Stream kmlStream,
    CancellationToken cancellationToken = default)
    {
        var imported = _kmlImporter.Parse(kmlStream);

        var resolvedName = !string.IsNullOrWhiteSpace(name)
            ? name
            : imported.SuggestedName;

        if (string.IsNullOrWhiteSpace(resolvedName))
        {
            throw new BusinessRuleException(
                "BEAT_NAME_REQUIRED",
                "A beat name is required and could not be determined from the KML file.");
        }

        var polygon = BeatGeometryValidator.BuildAndValidate(imported.BoundaryPoints);

        return await _repository.CreateAsync(
            _currentUser.TenantId,
            code,
            resolvedName,
            polygon,
            cancellationToken);
    }
}

