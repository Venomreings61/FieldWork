using FieldWork.Application.Repositories;
using FieldWork.Domain.Entities;
using FieldWork.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FieldWork.Infrastructure.Repositories;

public class FaceEmbeddingRepository : IFaceEmbeddingRepository
{
    private readonly FieldWorkDbContext _dbContext;

    public FaceEmbeddingRepository(FieldWorkDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FaceEmbedding?> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.FaceEmbeddings
            .Where(x => x.EmployeeId == employeeId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }


    public async Task AddAsync(FaceEmbedding faceEmbedding, CancellationToken cancellationToken = default)
    {
        await _dbContext.FaceEmbeddings.AddAsync(faceEmbedding, cancellationToken);
    }

    public Task UpdateAsync(FaceEmbedding faceEmbedding, CancellationToken cancellationToken = default)
    {
        _dbContext.FaceEmbeddings.Update(faceEmbedding);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(FaceEmbedding faceEmbedding, CancellationToken cancellationToken = default)
    {
        _dbContext.FaceEmbeddings.Remove(faceEmbedding);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}