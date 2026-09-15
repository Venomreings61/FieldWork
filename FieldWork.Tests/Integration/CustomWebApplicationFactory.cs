using FieldWork.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FieldWork.Tests.Integration
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            // Suppress duplicate console log streaming during test execution
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
            });

            // Integration tests run from the host machine.
            // The Docker Compose service name "face-service" is only
            // resolvable inside the Docker network.
            // Therefore tests connect to the published host port.
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["FaceService:BaseUrl"] = "http://localhost:8500"
                });
            });

            builder.ConfigureServices(services =>
            {
                // Remove existing DbContext registration.
                var descriptors = services
                    .Where(d =>
                        d.ServiceType == typeof(DbContextOptions<FieldWorkDbContext>) ||
                        d.ServiceType == typeof(FieldWorkDbContext))
                    .ToList();

                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                // Integration tests use the real PostgreSQL/PostGIS instance
                // exposed by Docker on localhost:5433.
                var testConnectionString =
                    "Host=localhost;Port=5433;Database=fieldwork;Username=fieldwork;Password=fieldwork_dev_pw;";

                services.AddDbContext<FieldWorkDbContext>(options =>
                {
                    options.UseNpgsql(testConnectionString, npgsqlOptions =>
                    {
                        npgsqlOptions.UseNetTopologySuite();
                        npgsqlOptions.UseVector();
                    });
                });
            });
        }
    }
}