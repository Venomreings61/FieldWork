using FieldWork.Application.DTOs;
using FieldWork.Application.DTOs.Face;

namespace FieldWork.Application.Services;

public interface IFaceEmployeeVerificationService
{
    Task<VerifyFaceResponse> VerifyFaceAsync(
        VerifyFaceRequest request,
        CancellationToken cancellationToken = default);
}