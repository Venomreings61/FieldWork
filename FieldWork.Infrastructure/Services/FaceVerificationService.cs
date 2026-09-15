using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FieldWork.Application.Exceptions;
using FieldWork.Application.Services;

namespace FieldWork.Infrastructure.Services;

public class FaceVerificationService : IFaceVerificationService
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public FaceVerificationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<EmbeddingResult> GenerateEmbeddingAsync(
        byte[] image,
        CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        using var imageContent = new ByteArrayContent(image);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(imageContent, "image", "image.jpg");

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync("/v1/face/embedding", content, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new FaceServiceUnavailableException(
                "FACE_SERVICE_UNAVAILABLE",
                $"Could not reach the face verification service: {ex.Message}");
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new FaceServiceUnavailableException(
                "FACE_SERVICE_TIMEOUT",
                $"The face verification service timed out: {ex.Message}");
        }

        if (response.StatusCode == HttpStatusCode.UnprocessableEntity ||
            response.StatusCode == HttpStatusCode.BadRequest)
        {
            var (errorCode, detail) = await ParseErrorResponseAsync(
                response,
                "FACE_ENROLLMENT_INPUT_ERROR",
                "The supplied image could not be used to generate a face embedding.",
                cancellationToken);

            throw new BusinessRuleException(errorCode, detail);
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new FaceServiceUnavailableException(
                "FACE_SERVICE_ERROR",
                $"Face verification service returned {(int)response.StatusCode}: {errorBody}");
        }

        PythonEmbeddingResponse? parsed;
        try
        {
            parsed = await response.Content.ReadFromJsonAsync<PythonEmbeddingResponse>(JsonOptions, cancellationToken);
        }
        catch (JsonException ex)
        {
            throw new FaceServiceUnavailableException(
                "FACE_SERVICE_INVALID_RESPONSE",
                $"Face verification service returned an unparsable response: {ex.Message}");
        }

        if (parsed is null || parsed.Embedding is null || parsed.Embedding.Length == 0)
        {
            throw new FaceServiceUnavailableException(
                "FACE_SERVICE_INVALID_RESPONSE",
                "Face verification service returned an empty embedding response.");
        }

        return new EmbeddingResult
        {
            Vector = parsed.Embedding,
            Model = parsed.Model,
            ModelVersion = string.IsNullOrWhiteSpace(parsed.ModelSha256) ? parsed.ModelVersion : parsed.ModelSha256
        };
    }

    public async Task<FaceVerificationResult> VerifyAsync(
     byte[] image,
     float[] referenceEmbedding,
     double? threshold,
     CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();

        // 1. Image
        using var imageContent = new ByteArrayContent(image);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(imageContent, "image", "image.jpg");

        // 2. Reference Embedding (JSON array string with exact alias: referenceEmbedding)
        var serializedEmbedding = JsonSerializer.Serialize(referenceEmbedding);
        content.Add(new StringContent(serializedEmbedding), "referenceEmbedding");

        // 3. Threshold
        if (threshold.HasValue)
        {
            content.Add(
                new StringContent(threshold.Value.ToString(CultureInfo.InvariantCulture)),
                "threshold");
        }

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync("/v1/face/verify", content, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new FaceServiceUnavailableException(
                "FACE_SERVICE_UNAVAILABLE",
                $"Could not reach the face verification service: {ex.Message}");
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new FaceServiceUnavailableException(
                "FACE_SERVICE_TIMEOUT",
                $"The face verification service timed out: {ex.Message}");
        }

        if (response.StatusCode == HttpStatusCode.UnprocessableEntity ||
            response.StatusCode == HttpStatusCode.BadRequest)
        {
            var (errorCode, detail) = await ParseErrorResponseAsync(
                response,
                "FACE_VERIFICATION_INPUT_ERROR",
                "The supplied image could not be processed for face verification.",
                cancellationToken);

            throw new BusinessRuleException(errorCode, detail);
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new FaceServiceUnavailableException(
                "FACE_SERVICE_ERROR",
                $"Face verification service returned {(int)response.StatusCode}: {errorBody}");
        }

        PythonVerificationResponse? parsed;
        try
        {
            parsed = await response.Content.ReadFromJsonAsync<PythonVerificationResponse>(JsonOptions, cancellationToken);
        }
        catch (JsonException ex)
        {
            throw new FaceServiceUnavailableException(
                "FACE_SERVICE_INVALID_RESPONSE",
                $"Face verification service returned an unparsable response: {ex.Message}");
        }

        if (parsed is null)
        {
            throw new FaceServiceUnavailableException(
                "FACE_SERVICE_INVALID_RESPONSE",
                "Face verification service returned an empty response.");
        }

        return new FaceVerificationResult
        {
            Matched = parsed.Matched,
            Similarity = parsed.Similarity,
            Threshold = threshold ?? parsed.Threshold
        };
    }

    private static async Task<(string ErrorCode, string Detail)> ParseErrorResponseAsync(
        HttpResponseMessage response,
        string defaultCode,
        string defaultDetail,
        CancellationToken cancellationToken)
    {
        try
        {
            var rawJson = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(rawJson);
            var root = doc.RootElement;

            string? extractedCode = null;
            string detail = defaultDetail;

            // Direct check for custom Python ErrorResponse schema: {"error_code": "...", "detail": "..."}
            if (root.TryGetProperty("error_code", out var errCodeProp) && errCodeProp.ValueKind == JsonValueKind.String)
            {
                extractedCode = errCodeProp.GetString();
            }
            else if (root.TryGetProperty("errorCode", out var errCodePropCamel) && errCodePropCamel.ValueKind == JsonValueKind.String)
            {
                extractedCode = errCodePropCamel.GetString();
            }

            if (root.TryGetProperty("detail", out var detailElem))
            {
                if (detailElem.ValueKind == JsonValueKind.String)
                {
                    detail = detailElem.GetString() ?? defaultDetail;
                }
                else if (detailElem.ValueKind == JsonValueKind.Array && detailElem.GetArrayLength() > 0)
                {
                    var fullDetailList = new List<string>();
                    foreach (var item in detailElem.EnumerateArray())
                    {
                        if (item.TryGetProperty("msg", out var msgVal))
                        {
                            fullDetailList.Add(msgVal.GetString() ?? string.Empty);
                        }
                    }
                    if (fullDetailList.Count > 0)
                    {
                        detail = string.Join("; ", fullDetailList);
                    }
                }
            }

            return (extractedCode ?? defaultCode, detail);
        }
        catch
        {
            return (defaultCode, defaultDetail);
        }
    }

    private class PythonEmbeddingResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("embedding")]
        public float[] Embedding { get; set; } = Array.Empty<float>();

        [JsonPropertyName("dimension")]
        public int Dimension { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("model_sha256")]
        public string ModelSha256 { get; set; } = string.Empty;

        [JsonPropertyName("model_version")]
        public string ModelVersion { get; set; } = string.Empty;
    }

    private class PythonVerificationResponse
    {
        [JsonPropertyName("matched")]
        public bool Matched { get; set; }

        [JsonPropertyName("similarity")]
        public double Similarity { get; set; }

        [JsonPropertyName("threshold")]
        public double Threshold { get; set; } = 0.6;
    }
}