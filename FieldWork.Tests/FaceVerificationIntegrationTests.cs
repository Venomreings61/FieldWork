using System.Net.Http.Json;
using System.Text.Json;
using FieldWork.Application.Exceptions;
using FieldWork.Infrastructure.Services;
using Xunit;

namespace FieldWork.Tests;

public class FaceVerificationIntegrationTests : IAsyncLifetime
{
    private const string FaceServiceBaseUrl = "http://localhost:8500";

    private static readonly string TestDataDir =
        Path.Combine(AppContext.BaseDirectory, "TestData");

    private float[] _personAReferenceEmbedding = Array.Empty<float>();
    private HttpClient _rawPythonClient = null!;

    public async Task InitializeAsync()
    {
        _rawPythonClient = new HttpClient { BaseAddress = new Uri(FaceServiceBaseUrl) };

        var readyResponse = await _rawPythonClient.GetAsync("/health/ready");
        if (!readyResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                "Face service is not ready. Ensure `docker compose up -d` is running " +
                "before executing these integration tests.");
        }

        var service = CreateService();
        var personABytes = await File.ReadAllBytesAsync(Path.Combine(TestDataDir, "personA_1.png"));
        var result = await service.GenerateEmbeddingAsync(personABytes);
        _personAReferenceEmbedding = result.Vector;
    }

    public Task DisposeAsync()
    {
        _rawPythonClient.Dispose();
        return Task.CompletedTask;
    }

    private static FaceVerificationService CreateService()
    {
        var httpClient = new HttpClient { BaseAddress = new Uri(FaceServiceBaseUrl) };
        return new FaceVerificationService(httpClient);
    }

    [Fact]
    public async Task GenerateEmbeddingAsync_ValidImage_ReturnsEmbeddingAndModelDetails()
    {
        var service = CreateService();
        var imageBytes = await File.ReadAllBytesAsync(Path.Combine(TestDataDir, "personA_1.png"));

        var result = await service.GenerateEmbeddingAsync(imageBytes);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Vector);
        Assert.NotEmpty(result.Model);
        Assert.NotEmpty(result.ModelVersion);
    }

    [Fact]
    public async Task SamePerson_DefaultThreshold_Matches()
    {
        var service = CreateService();
        var candidateBytes = await File.ReadAllBytesAsync(Path.Combine(TestDataDir, "personA_2.png"));

        var result = await service.VerifyAsync(candidateBytes, _personAReferenceEmbedding, threshold: null);

        Assert.True(result.Matched);
        Assert.True(result.Similarity >= result.Threshold);
        Assert.InRange(result.Threshold, 0.0, 1.0);
    }

    [Fact]
    public async Task DifferentPerson_DefaultThreshold_DoesNotMatch()
    {
        var service = CreateService();
        var candidateBytes = await File.ReadAllBytesAsync(Path.Combine(TestDataDir, "personB_1.png"));

        var result = await service.VerifyAsync(candidateBytes, _personAReferenceEmbedding, threshold: null);

        Assert.False(result.Matched);
        Assert.True(result.Similarity < result.Threshold);
    }

    [Fact]
    public async Task SamePerson_LowThreshold_Matches()
    {
        var service = CreateService();
        var candidateBytes = await File.ReadAllBytesAsync(Path.Combine(TestDataDir, "personA_2.png"));

        var result = await service.VerifyAsync(candidateBytes, _personAReferenceEmbedding, threshold: 0.1);

        Assert.True(result.Matched);
        Assert.Equal(0.1, result.Threshold, precision: 2);
    }

    [Fact]
    public async Task SamePerson_HighThreshold_DoesNotMatch()
    {
        var service = CreateService();
        var candidateBytes = await File.ReadAllBytesAsync(Path.Combine(TestDataDir, "personA_2.png"));

        var result = await service.VerifyAsync(candidateBytes, _personAReferenceEmbedding, threshold: 0.99);

        Assert.False(result.Matched);
        Assert.Equal(0.99, result.Threshold, precision: 2);
    }

    [Fact]
    public async Task NoFaceInImage_ThrowsBusinessRuleException()
    {
        var service = CreateService();
        var blankPngPath = Path.Combine(TestDataDir, "blank.png");
        var blankBytes = await File.ReadAllBytesAsync(blankPngPath);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.VerifyAsync(blankBytes, _personAReferenceEmbedding, threshold: null));

        Assert.Equal("NO_FACE_DETECTED", exception.Code);
    }

    [Fact]
    public async Task MultipleFacesInImage_ThrowsBusinessRuleException()
    {
        var service = CreateService();
        var multiFacePath = Path.Combine(TestDataDir, "two_faces.png");
        var bytes = await File.ReadAllBytesAsync(multiFacePath);

        var exception = await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.VerifyAsync(bytes, _personAReferenceEmbedding, threshold: null));

        Assert.Equal("MULTIPLE_FACES_DETECTED", exception.Code);
    }
}