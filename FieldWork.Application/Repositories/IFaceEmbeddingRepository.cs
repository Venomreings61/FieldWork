using FieldWork.Domain.Entities;

namespace FieldWork.Application.Repositories;

public interface IFaceEmbeddingRepository
{
    Task<FaceEmbedding?> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task AddAsync(FaceEmbedding faceEmbedding, CancellationToken cancellationToken = default);
    Task UpdateAsync(FaceEmbedding faceEmbedding, CancellationToken cancellationToken = default);
    Task DeleteAsync(FaceEmbedding faceEmbedding, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}