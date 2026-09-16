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

            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["FaceService:BaseUrl"] = "http://localhost:8500",
                    ["Seed:Enabled"] = "true"
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

        // Ensure database migrations and seeding run as soon as the WebApplicationFactory starts up
        public CustomWebApplicationFactory()
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<FieldWorkDbContext>();

            // Apply migrations so 'users', 'employees', etc. are created
            db.Database.Migrate();

            // Optional: If you have a custom seeder method, call it here, 
            // e.g., DbSeeder.SeedAsync(db).GetAwaiter().GetResult();
        }
    }
}