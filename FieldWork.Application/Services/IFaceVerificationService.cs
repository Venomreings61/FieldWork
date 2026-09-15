namespace FieldWork.Application.Services;

public interface IFaceVerificationService
{
    Task<FaceVerificationResult> VerifyAsync(
        byte[] image,
        float[] referenceEmbedding,
        double? threshold,
        CancellationToken cancellationToken = default);

    Task<EmbeddingResult> GenerateEmbeddingAsync(
        byte[] image,
        CancellationToken cancellationToken = default);
}

public class EmbeddingResult
{
    public float[] Vector { get; set; } = Array.Empty<float>();
    public string Model { get; set; } = string.Empty;

    // The SHA-256 hash reported by Python acts as the precise version identifier
    // for audit and reproducibility.
    public string ModelVersion { get; set; } = string.Empty;
}