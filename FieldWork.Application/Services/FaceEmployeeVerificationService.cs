using FieldWork.Application.DTOs;
using FieldWork.Application.DTOs.Face;
using FieldWork.Application.Exceptions;
using FieldWork.Application.Repositories;
using FieldWork.Application.Security;

namespace FieldWork.Application.Services;

public class FaceEmployeeVerificationService : IFaceEmployeeVerificationService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IFaceEmbeddingRepository _faceEmbeddingRepository;
    private readonly IFaceVerificationService _faceVerificationService;
    private readonly ICurrentUser _currentUser;

    public FaceEmployeeVerificationService(
        IEmployeeRepository employeeRepository,
        IFaceEmbeddingRepository faceEmbeddingRepository,
        IFaceVerificationService faceVerificationService,
        ICurrentUser currentUser)
    {
        _employeeRepository = employeeRepository;
        _faceEmbeddingRepository = faceEmbeddingRepository;
        _faceVerificationService = faceVerificationService;
        _currentUser = currentUser;
    }

    public async Task<VerifyFaceResponse> VerifyFaceAsync(
        VerifyFaceRequest request,
        CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(
            request.EmployeeId, _currentUser.TenantId, cancellationToken)
            ?? throw new NotFoundException(
                "EMPLOYEE_NOT_FOUND",
                "Employee was not found in the current tenant.");

        if (!IsAdmin())
        {
            var callerEmployee = await _employeeRepository.GetByUserIdAsync(
                _currentUser.UserId, _currentUser.TenantId, cancellationToken);

            if (callerEmployee is null || callerEmployee.Id != employee.Id)
            {
                throw new ForbiddenException(
                    "FACE_VERIFICATION_FORBIDDEN",
                    "You are not authorized to verify this employee.");
            }
        }

        if (!employee.IsActive)
        {
            throw new BusinessRuleException(
                "EMPLOYEE_INACTIVE",
                "Cannot verify face for an inactive employee.");
        }

        var storedEmbedding = await _faceEmbeddingRepository.GetByEmployeeIdAsync(
            employee.Id, cancellationToken)
            ?? throw new NotFoundException(
                "FACE_NOT_ENROLLED",
                "Employee does not have an enrolled face.");

        var referenceEmbedding = storedEmbedding.Embedding.ToArray();

        var result = await _faceVerificationService.VerifyAsync(
            request.ImageBytes, referenceEmbedding, request.Threshold, cancellationToken);

        return new VerifyFaceResponse(result.Matched, result.Similarity, result.Threshold);
    }

    private bool IsAdmin() => _currentUser.Role == "Admin";
}