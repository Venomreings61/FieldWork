namespace FieldWork.Application.DTOs.Face;

public record EnrollFaceRequest(
    Guid EmployeeId,
    byte[] ImageBytes
);

public record EnrollFaceResponse(
    Guid Id,
    Guid EmployeeId,
    string Model,
    string ModelVersion,
    DateTime UpdatedAt
);

public record VerifyFaceRequest(
    Guid EmployeeId,
    byte[] ImageBytes,
    double? Threshold = null
);

public record VerifyFaceResponse(
    bool Matched,
    double Similarity,
    double Threshold
);
