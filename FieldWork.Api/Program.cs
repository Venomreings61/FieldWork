using FieldWork.Api.HealthChecks;
using FieldWork.Api.Middleware;
using FieldWork.Application.Authentication;
using FieldWork.Application.Repositories;
using FieldWork.Application.Security;
using FieldWork.Application.Services;
using FieldWork.Infrastructure.Data;
using FieldWork.Infrastructure.HealthChecks;
using FieldWork.Infrastructure.Repositories;
using FieldWork.Infrastructure.Security;
using FieldWork.Infrastructure.Seed;
using FieldWork.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using FieldWork.Application.Configuration;

var builder = WebApplication.CreateBuilder(args);

// 1. Controllers & JSON Options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// 2. Database Context
var dbConnectionString = builder.Configuration.GetConnectionString("FieldWorkDb")
    ?? throw new InvalidOperationException("Database connection string 'FieldWorkDb' is missing.");

builder.Services.AddDbContext<FieldWorkDbContext>(options =>
    options.UseNpgsql(dbConnectionString, npgsqlOptions =>
    {
        npgsqlOptions.UseNetTopologySuite();
        npgsqlOptions.UseVector();
    }));

// 3. Health Checks Configuration
builder.Services.AddHealthChecks()
    .AddNpgSql(
        dbConnectionString,
        name: "postgresql",
        tags: new[] { "ready" },
        timeout: TimeSpan.FromSeconds(5))
    .AddCheck<PostGisHealthCheck>(
        name: "postgis",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "ready" },
        timeout: TimeSpan.FromSeconds(5));

// 4. CORS Policy Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// 5. Application Services & Repositories
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IBeatRepository, BeatRepository>();
builder.Services.AddScoped<IEmployeeBeatRepository, EmployeeBeatRepository>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IBeatService, BeatService>();
builder.Services.AddScoped<IEmployeeBeatService, EmployeeBeatService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IGeofenceRepository, GeofenceRepository>();
builder.Services.AddScoped<IBeatKmlImporter, KmlBeatImporter>();
builder.Services.AddScoped<IFaceEmbeddingRepository, FaceEmbeddingRepository>();
builder.Services.AddScoped<IFaceEnrollmentService, FaceEnrollmentService>();
builder.Services.AddScoped<IFaceEmployeeVerificationService, FaceEmployeeVerificationService>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<ITenantService, TenantService>();

builder.Services.Configure<FaceVerificationOptions>(builder.Configuration.GetSection("FaceService"));

builder.Services.AddHttpClient<IFaceVerificationService, FaceVerificationService>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<FaceVerificationOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});

// 6. JWT Authentication
var jwtSettings = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtSettings>()
    ?? throw new InvalidOperationException("JWT settings are missing.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero, // Eliminates default 5-min grace period on token expiration

            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtAuthentication");

                logger.LogWarning(context.Exception, "JWT authentication failed.");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// 7. Swagger / OpenAPI Configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT Bearer token."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

var app = builder.Build();

// 8. Global Middleware Pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

var seedEnabled = builder.Configuration.GetValue<bool>("Seed:Enabled");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FieldWork API v1");
        c.RoutePrefix = "swagger";
    });
}

if (seedEnabled)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<FieldWorkDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db, passwordHasher);

    var devHash = passwordHasher.Hash("Password@123");
    logger.LogInformation("Seed setup complete. Sample Hash: {Hash}", devHash);
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// 9. Health Check Endpoints (Bypasses authentication filters by default)
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("ready"),
    ResponseWriter = HealthResponseWriter.WriteJson,
    AllowCachingResponses = false
});

if (app.Environment.IsDevelopment())
{
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = HealthResponseWriter.WriteJson,
        AllowCachingResponses = false
    });
}

// 10. Controller Route Mapping
app.MapControllers();

app.Run();

public partial class Program { }