using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FieldWork.Infrastructure.Data;
using Xunit;

namespace FieldWork.Tests.Integration
{
    [Collection("SequentialAttendanceTests")]
    public class AttendanceApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public AttendanceApiIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<string> AuthenticateAsync()
        {
            var loginPayload = new { username = "employee01", password = "Password@123" };
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginPayload);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Authentication failed with status {response.StatusCode}. Details: {errorBody}");
            }

            var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            Assert.NotNull(result?.AccessToken);
            return result.AccessToken;
        }

        private async Task<string> AuthenticateAsAdminAsync()
        {
            var loginPayload = new { username = "admin", password = "Password@123" };
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginPayload);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            Assert.NotNull(result?.AccessToken);
            return result.AccessToken;
        }

        private async Task<Guid> GetEmployeeIdAsync(string username)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<FieldWorkDbContext>();
            var employee = await db.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.User.Username == username);
            Assert.NotNull(employee);
            return employee.Id;
        }

        private async Task<Guid> GetTenantIdAsync(string code)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<FieldWorkDbContext>();
            var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Code == code);
            Assert.NotNull(tenant);
            return tenant.Id;
        }

        private async Task EnsureFaceEnrolledAsync(Guid employeeId, string adminToken, string imageFileName = "personA_1.png")
        {
            using var enrollClient = _factory.CreateClient();
            enrollClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
            var imagePath = Path.Combine(AppContext.BaseDirectory, "TestData", imageFileName);
            using var content = new MultipartFormDataContent();
            using var fileStream = File.OpenRead(imagePath);
            using var streamContent = new StreamContent(fileStream);
            content.Add(streamContent, "image", imageFileName);
            var response = await enrollClient.PostAsync($"/api/v1/employees/{employeeId}/face/enroll", content);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                Assert.Fail($"Face enrollment setup failed: {response.StatusCode} - {body}");
            }
        }

        private async Task<string> DetermineNextActionAsync()
        {
            var response = await _client.GetFromJsonAsync<AttendanceListResponseDto>("/api/attendance");
            var latestAction = response?.Items?.FirstOrDefault()?.Action;

            return latestAction switch
            {
                "CHECK_IN" => "CHECK_OUT",
                "CHECK_OUT" => "CHECK_IN",
                _ => "CHECK_IN"
            };
        }

        [Fact]
        public async Task CreateAttendance_MissingFaceImage_WhenFaceRequired_ReturnsBadRequest()
        {
            var token = await AuthenticateAsync();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var nextAction = await DetermineNextActionAsync();

            var payload = new
            {
                action = nextAction,
                source = "GPS",
                latitude = 19.0760,
                longitude = 72.8777,
                clientAttendanceId = Guid.NewGuid().ToString(),
                recordedAt = DateTime.UtcNow
            };

            var response = await _client.PostAsJsonAsync("/api/attendance", payload);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateAttendance_NoEnrollment_WhenFaceRequired_ReturnsNotFound()
        {
            var token = await AuthenticateAsync();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var nextAction = await DetermineNextActionAsync();

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FieldWorkDbContext>();

            var employeeIdGuid = await GetEmployeeIdAsync("employee01");

            var existingEmbedding = await dbContext.Database
                .SqlQueryRaw<FaceEmbeddingRow>(
                    "SELECT \"Id\", \"EmployeeId\", \"Embedding\"::text AS \"EmbeddingText\", \"Model\", \"ModelVersion\", \"CreatedAt\", \"UpdatedAt\" FROM employee_face_embeddings WHERE \"EmployeeId\" = {0}",
                    employeeIdGuid)
                .FirstOrDefaultAsync();

            await dbContext.Database.ExecuteSqlRawAsync(
                "DELETE FROM employee_face_embeddings WHERE \"EmployeeId\" = {0}", employeeIdGuid);

            try
            {
                var imagePath = Path.Combine(AppContext.BaseDirectory, "TestData", "personA_2.png");
                var faceImageBytes = await File.ReadAllBytesAsync(imagePath);
                var faceImageBase64 = Convert.ToBase64String(faceImageBytes);

                var payload = new
                {
                    action = nextAction,
                    source = "GPS",
                    latitude = 19.0760,
                    longitude = 72.8777,
                    clientAttendanceId = Guid.NewGuid().ToString(),
                    recordedAt = DateTime.UtcNow,
                    faceImageBase64
                };

                var response = await _client.PostAsJsonAsync("/api/attendance", payload);
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

                var responseContent = await response.Content.ReadAsStringAsync();
                Assert.Contains("FACE_NOT_ENROLLED", responseContent);
            }
            finally
            {
                if (existingEmbedding != null)
                {
                    await dbContext.Database.ExecuteSqlRawAsync(
                        "INSERT INTO employee_face_embeddings (\"Id\", \"EmployeeId\", \"Embedding\", \"Model\", \"ModelVersion\", \"CreatedAt\", \"UpdatedAt\") " +
                        "VALUES ({0}, {1}, cast({2} as vector), {3}, {4}, {5}, {6}) " +
                        "ON CONFLICT (\"EmployeeId\") DO NOTHING;",
                        existingEmbedding.Id,
                        existingEmbedding.EmployeeId,
                        existingEmbedding.EmbeddingText,
                        existingEmbedding.Model,
                        existingEmbedding.ModelVersion,
                        existingEmbedding.CreatedAt,
                        existingEmbedding.UpdatedAt);
                }
            }
        }

        [Fact]
        public async Task CreateAttendance_WithoutFace_WhenFaceDisabled_ReturnsSuccess()
        {
            var token = await AuthenticateAsync();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var nextAction = await DetermineNextActionAsync();

            var tenantId = await GetTenantIdAsync("FIELDWORK");

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FieldWorkDbContext>();

            var tenant = await dbContext.Tenants.FindAsync(tenantId);
            Assert.NotNull(tenant);
            var originalFaceVerificationRequired = tenant.FaceVerificationRequired;

            tenant.FaceVerificationRequired = false;
            await dbContext.SaveChangesAsync();

            try
            {
                var payload = new
                {
                    action = nextAction,
                    source = "GPS",
                    latitude = 19.0760,
                    longitude = 72.8777,
                    clientAttendanceId = Guid.NewGuid().ToString(),
                    recordedAt = DateTime.UtcNow
                };

                var response = await _client.PostAsJsonAsync("/api/attendance", payload);
                response.EnsureSuccessStatusCode();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
            finally
            {
                tenant.FaceVerificationRequired = originalFaceVerificationRequired;
                dbContext.Update(tenant);
                await dbContext.SaveChangesAsync();
            }
        }

        [Fact]
        public async Task CreateAttendance_FaceMismatch_WhenFaceRequired_ReturnsBadRequest()
        {
            var employeeId = await GetEmployeeIdAsync("employee01");
            var adminToken = await AuthenticateAsAdminAsync();
            await EnsureFaceEnrolledAsync(employeeId, adminToken);

            var token = await AuthenticateAsync();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var nextAction = await DetermineNextActionAsync();

            var imagePath = Path.Combine(AppContext.BaseDirectory, "TestData", "personB_1.png");
            var faceImageBytes = await File.ReadAllBytesAsync(imagePath);
            var faceImageBase64 = Convert.ToBase64String(faceImageBytes);

            var payload = new
            {
                action = nextAction,
                source = "GPS",
                latitude = 19.0760,
                longitude = 72.8777,
                clientAttendanceId = Guid.NewGuid().ToString(),
                recordedAt = DateTime.UtcNow,
                faceImageBase64
            };

            var response = await _client.PostAsJsonAsync("/api/attendance", payload);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("ATTENDANCE_FACE_VERIFICATION_FAILED", responseContent);
        }

        [Fact]
        public async Task CreateAttendance_CorrectFaceOutsideGeofence_ReturnsBadRequest()
        {
            var token = await AuthenticateAsync();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var nextAction = await DetermineNextActionAsync();

            var imagePath = Path.Combine(AppContext.BaseDirectory, "TestData", "personA_2.png");
            var faceImageBytes = await File.ReadAllBytesAsync(imagePath);
            var faceImageBase64 = Convert.ToBase64String(faceImageBytes);

            var payload = new
            {
                action = nextAction,
                source = "GPS",
                latitude = 0.0000,
                longitude = 0.0000,
                clientAttendanceId = Guid.NewGuid().ToString(),
                recordedAt = DateTime.UtcNow,
                faceImageBase64
            };

            var clientAttendanceId = payload.clientAttendanceId;

            var response = await _client.PostAsJsonAsync("/api/attendance", payload);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("ATTENDANCE_OUTSIDE_GEOFENCE", responseContent);

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FieldWorkDbContext>();
            var attendanceExists = await dbContext.Attendances
                .AnyAsync(a => a.ClientAttendanceId == clientAttendanceId);
            Assert.False(attendanceExists);
        }

        [Fact]
        public async Task CreateAttendance_DuplicateClientAttendanceId_ReturnsSameAttendance()
        {
            var employeeId = await GetEmployeeIdAsync("employee01");
            var adminToken = await AuthenticateAsAdminAsync();
            await EnsureFaceEnrolledAsync(employeeId, adminToken);

            var token = await AuthenticateAsync();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            using (var cleanupScope = _factory.Services.CreateScope())
            {
                var db = cleanupScope.ServiceProvider.GetRequiredService<FieldWorkDbContext>();
                var existingAttendances = db.Attendances.Where(a => a.EmployeeId == employeeId);
                db.Attendances.RemoveRange(existingAttendances);
                await db.SaveChangesAsync();
            }

            string targetAction = "CHECK_IN";
            var imagePath = Path.Combine(AppContext.BaseDirectory, "TestData", "personA_1.png");
            var faceImageBytes = await File.ReadAllBytesAsync(imagePath);
            var faceImageBase64 = Convert.ToBase64String(faceImageBytes);

            var sharedClientAttendanceId = Guid.NewGuid().ToString();

            var payload = new
            {
                action = targetAction,
                source = "GPS",
                latitude = 19.0760,
                longitude = 72.8777,
                clientAttendanceId = sharedClientAttendanceId,
                recordedAt = DateTime.UtcNow,
                faceImageBase64
            };

            var response1 = await _client.PostAsJsonAsync("/api/attendance", payload);
            if (!response1.IsSuccessStatusCode)
            {
                var errorContent = await response1.Content.ReadAsStringAsync();
                Assert.Fail(
                    $"First request failed with {response1.StatusCode} " +
                    $"(Action attempted: {targetAction}): " +
                    $"{errorContent}");
            }

            var result1 = await response1.Content.ReadFromJsonAsync<AttendanceResponseDto>();
            Assert.NotNull(result1);

            var response2 = await _client.PostAsJsonAsync("/api/attendance", payload);
            response2.EnsureSuccessStatusCode();

            var result2 = await response2.Content.ReadFromJsonAsync<AttendanceResponseDto>();
            Assert.NotNull(result2);

            Assert.Equal(result1.Id, result2.Id);
            Assert.Equal(sharedClientAttendanceId, result2.ClientAttendanceId);

            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FieldWorkDbContext>();
            var count = await dbContext.Attendances
                .CountAsync(a => a.ClientAttendanceId == sharedClientAttendanceId);

            Assert.Equal(1, count);
        }

        [Fact]
        public async Task CreateAttendance_StaleRecordedAt_PreservesClientTime()
        {
            var employeeId = await GetEmployeeIdAsync("employee01");
            var adminToken = await AuthenticateAsAdminAsync();
            await EnsureFaceEnrolledAsync(employeeId, adminToken);

            var token = await AuthenticateAsync();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var nextAction = await DetermineNextActionAsync();

            var imagePath = Path.Combine(AppContext.BaseDirectory, "TestData", "personA_1.png");
            var faceImageBytes = await File.ReadAllBytesAsync(imagePath);
            var faceImageBase64 = Convert.ToBase64String(faceImageBytes);

            var staleRecordedAt = DateTimeOffset.UtcNow.AddHours(-20);
            var beforeRequest = DateTimeOffset.UtcNow;

            var payload = new
            {
                action = nextAction,
                source = "GPS",
                latitude = 19.0760,
                longitude = 72.8777,
                clientAttendanceId = Guid.NewGuid().ToString(),
                recordedAt = staleRecordedAt,
                faceImageBase64
            };

            var response = await _client.PostAsJsonAsync("/api/attendance", payload);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<AttendanceDetailDto>();
            Assert.NotNull(result);

            // Compare by absolute instant (UtcDateTime), not by Kind/offset representation.
            Assert.Equal(staleRecordedAt.UtcDateTime, result.RecordedAt.UtcDateTime, TimeSpan.FromSeconds(1));
            Assert.True(result.ReceivedAt >= beforeRequest);
            Assert.True(result.ReceivedAt.UtcDateTime > result.RecordedAt.UtcDateTime.AddHours(1));
            Assert.True(result.IsWithinGeofence);
            Assert.Equal("Synced", result.SyncStatus);
        }

        [Fact]
        public async Task CreateAttendance_ConcurrentRequests_WithFace_AllowsOnlyOne()
        {
            var employeeId = await GetEmployeeIdAsync("employee01");
            var adminToken = await AuthenticateAsAdminAsync();
            await EnsureFaceEnrolledAsync(employeeId, adminToken);

            var token = await AuthenticateAsync();
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Pin a known, deterministic starting state BEFORE firing concurrent
            // requests — don't rely on DetermineNextActionAsync() read immediately
            // before the race, since that read-then-fire gap is itself a potential
            // source of test flakiness independent of the code under test.
            var setupAction = await DetermineNextActionAsync();
            var imagePathSetup = Path.Combine(AppContext.BaseDirectory, "TestData", "personA_1.png");
            var faceImageBase64Setup = Convert.ToBase64String(await File.ReadAllBytesAsync(imagePathSetup));

            var setupResponse = await _client.PostAsJsonAsync("/api/attendance", new
            {
                action = setupAction,
                source = "GPS",
                latitude = 19.0760,
                longitude = 72.8777,
                clientAttendanceId = Guid.NewGuid().ToString(),
                recordedAt = DateTime.UtcNow,
                faceImageBase64 = faceImageBase64Setup
            });
            Assert.Equal(HttpStatusCode.OK, setupResponse.StatusCode);

            // Employee is now definitively in `setupAction` state. All 5 concurrent
            // requests attempt the SAME opposite action — exactly one should win.
            var raceAction = setupAction == "CHECK_IN" ? "CHECK_OUT" : "CHECK_IN";
            var imagePath = Path.Combine(AppContext.BaseDirectory, "TestData", "personA_2.png");
            var faceImageBase64 = Convert.ToBase64String(await File.ReadAllBytesAsync(imagePath));

            var clientIds = Enumerable.Range(1, 5)
                .Select(_ => Guid.NewGuid().ToString())
                .ToList();

            // Separate HttpClient per request — sharing one HttpClient across truly
            // concurrent requests is fine for HttpClient itself (it's thread-safe),
            // but using the factory's client-creation per request avoids any shared
            // mutable state (e.g. DefaultRequestHeaders) becoming a false source of
            // interference between "concurrent" calls.
            var tasks = clientIds.Select(async clientId =>
            {
                using var raceClient = _factory.CreateClient();
                raceClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var payload = new
                {
                    action = raceAction,
                    source = "GPS",
                    latitude = 19.0760,
                    longitude = 72.8777,
                    clientAttendanceId = clientId,
                    recordedAt = DateTime.UtcNow,
                    faceImageBase64
                };

                var response = await raceClient.PostAsJsonAsync("/api/attendance", payload);
                var body = await response.Content.ReadAsStringAsync();
                return (StatusCode: response.StatusCode, Body: body);
            }).ToList();

            var results = await Task.WhenAll(tasks);

            var successCount = results.Count(r => r.StatusCode == HttpStatusCode.OK);
            var conflictCount = results.Count(r => r.StatusCode == HttpStatusCode.Conflict);

            Assert.Equal(1, successCount);
            Assert.Equal(4, conflictCount);

            var expectedConflictCode = raceAction == "CHECK_IN" ? "ATTENDANCE_ALREADY_CHECKED_IN" : "ATTENDANCE_ALREADY_CHECKED_OUT";
            Assert.All(
                results.Where(r => r.StatusCode == HttpStatusCode.Conflict),
                r => Assert.Contains(expectedConflictCode, r.Body));

            // Confirm exactly one new row was actually created from this batch of 5.
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FieldWorkDbContext>();
            var createdCount = await dbContext.Attendances
                .CountAsync(a => clientIds.Contains(a.ClientAttendanceId));

            Assert.Equal(1, createdCount);
        }
    }

    public record FaceEmbeddingRow(
        Guid Id,
        Guid EmployeeId,
        string EmbeddingText,
        string Model,
        string ModelVersion,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );

    public record LoginResponseDto([property: JsonPropertyName("accessToken")] string AccessToken);
    public record AttendanceListResponseDto([property: JsonPropertyName("items")] List<AttendanceItemDto> Items);
    public record AttendanceItemDto([property: JsonPropertyName("action")] string Action);
    public record AttendanceResponseDto(
        [property: JsonPropertyName("id")] Guid Id,
        [property: JsonPropertyName("clientAttendanceId")] string ClientAttendanceId,
        [property: JsonPropertyName("action")] string Action
    );
    public record AttendanceDetailDto(
        [property: JsonPropertyName("id")] Guid Id,
        [property: JsonPropertyName("clientAttendanceId")] string ClientAttendanceId,
        [property: JsonPropertyName("action")] string Action,
        [property: JsonPropertyName("recordedAt")] DateTimeOffset RecordedAt,
        [property: JsonPropertyName("receivedAt")] DateTimeOffset ReceivedAt,
        [property: JsonPropertyName("isWithinGeofence")] bool IsWithinGeofence,
        [property: JsonPropertyName("syncStatus")] string SyncStatus
    );
}