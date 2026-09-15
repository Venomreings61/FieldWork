using FieldWork.Application.DTOs;
using FieldWork.Application.DTOs.Face;

namespace FieldWork.Application.Services;

public interface IFaceEnrollmentService
{
    Task<EnrollFaceResponse> EnrollFaceAsync(EnrollFaceRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteFaceEnrollmentAsync(Guid employeeId, CancellationToken cancellationToken = default);
}