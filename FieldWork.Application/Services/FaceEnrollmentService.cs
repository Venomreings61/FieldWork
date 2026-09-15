using FieldWork.Application.DTOs;
using FieldWork.Application.DTOs.Face;
using FieldWork.Application.Exceptions;
using FieldWork.Application.Repositories;
using FieldWork.Application.Security;
using FieldWork.Domain.Entities;
using Pgvector;

namespace FieldWork.Application.Services;

public class FaceEnrollmentService : IFaceEnrollmentService
{
    private readonly IFaceEmbeddingRepository _faceEmbeddingRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IFaceVerificationService _faceVerificationService;
    private readonly ICurrentUser _currentUser;

    public FaceEnrollmentService(
        IFaceEmbeddingRepository faceEmbeddingRepository,
        IEmployeeRepository employeeRepository,
        IFaceVerificationService faceVerificationService,
        ICurrentUser currentUser)
    {
        _faceEmbeddingRepository = faceEmbeddingRepository;
        _employeeRepository = employeeRepository;
        _faceVerificationService = faceVerificationService;
        _currentUser = currentUser;
    }

    public async Task<EnrollFaceResponse> EnrollFaceAsync(
        EnrollFaceRequest request,
        CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(
            request.EmployeeId, _currentUser.TenantId, cancellationToken)
            ?? throw new NotFoundException(
                "EMPLOYEE_NOT_FOUND",
                "Employee was not found in the current tenant.");

        var embeddingResult = await _faceVerificationService.GenerateEmbeddingAsync(
            request.ImageBytes, cancellationToken);

        var existingEmbedding = await _faceEmbeddingRepository.GetByEmployeeIdAsync(
            employee.Id, cancellationToken);

        if (existingEmbedding is not null)
        {
            existingEmbedding.Embedding = new Vector(embeddingResult.Vector);
            existingEmbedding.Model = embeddingResult.Model;
            existingEmbedding.ModelVersion = embeddingResult.ModelVersion;
            existingEmbedding.UpdatedAt = DateTime.UtcNow;

            await _faceEmbeddingRepository.UpdateAsync(existingEmbedding, cancellationToken);
            await _faceEmbeddingRepository.SaveChangesAsync(cancellationToken);

            return new EnrollFaceResponse(
                existingEmbedding.Id,
                existingEmbedding.EmployeeId,
                existingEmbedding.Model,
                existingEmbedding.ModelVersion,
                existingEmbedding.UpdatedAt);
        }

        var newEmbedding = new FaceEmbedding
        {
            Id = Guid.NewGuid(),
            EmployeeId = employee.Id,
            Embedding = new Vector(embeddingResult.Vector),
            Model = embeddingResult.Model,
            ModelVersion = embeddingResult.ModelVersion,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _faceEmbeddingRepository.AddAsync(newEmbedding, cancellationToken);
        await _faceEmbeddingRepository.SaveChangesAsync(cancellationToken);

        return new EnrollFaceResponse(
            newEmbedding.Id,
            newEmbedding.EmployeeId,
            newEmbedding.Model,
            newEmbedding.ModelVersion,
            newEmbedding.CreatedAt);
    }

    public async Task<bool> DeleteFaceEnrollmentAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(
            employeeId, _currentUser.TenantId, cancellationToken)
            ?? throw new NotFoundException(
                "EMPLOYEE_NOT_FOUND",
                "Employee was not found in the current tenant.");

        var embedding = await _faceEmbeddingRepository.GetByEmployeeIdAsync(employee.Id, cancellationToken);
        if (embedding is null)
        {
            return false;
        }

        await _faceEmbeddingRepository.DeleteAsync(embedding, cancellationToken);
        await _faceEmbeddingRepository.SaveChangesAsync(cancellationToken);
        return true;
    }
}