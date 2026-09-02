using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace FieldWork.Infrastructure.HealthChecks;

public sealed class PostGisHealthCheck : IHealthCheck
{
    private const string Sql = """
        SELECT EXISTS (
            SELECT 1
            FROM pg_extension
            WHERE extname = 'postgis'
        );
        """;

    private readonly string _connectionString;

    public PostGisHealthCheck(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("FieldWorkDb")
            ?? throw new InvalidOperationException("Database connection string 'FieldWorkDb' is missing.");
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
    HealthCheckContext context,
    CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT postgis_full_version();";

            var version = await command.ExecuteScalarAsync(cancellationToken);

            if (version != null && version != DBNull.Value)
            {
                return HealthCheckResult.Healthy("PostGIS extension is active.");
            }

            return HealthCheckResult.Unhealthy("PostGIS version check returned null.");
        }
        catch (OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy("PostGIS check timed out waiting for database connection.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"PostGIS check failed: {ex.Message}");
        }
    }
}