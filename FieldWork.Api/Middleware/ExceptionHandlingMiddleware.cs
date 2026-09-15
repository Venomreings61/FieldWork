using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using FieldWork.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FieldWork.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var (statusCode, title, code) = MapException(ex);

            var tenantId = context.User.FindFirst("tenantId")?.Value ?? "unknown";
            var userId = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? "anonymous";

            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["TenantId"] = tenantId,
                ["UserId"] = userId,
                ["RequestPath"] = context.Request.Path.ToString(),
                ["TraceId"] = context.TraceIdentifier
            }))
            {
                if (statusCode is StatusCodes.Status500InternalServerError or StatusCodes.Status503ServiceUnavailable)
                {
                    _logger.LogError(ex, "Service or server error occurred.");
                }
                else
                {
                    _logger.LogWarning(ex, "{Title}", title);
                }
            }

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = ex.Message,
                Type = $"https://tools.ietf.org/html/rfc9110#section-{RfcSectionFor(statusCode)}",
                Instance = context.Request.Path
            };

            problemDetails.Extensions["code"] = code;
            problemDetails.Extensions["traceId"] = context.TraceIdentifier;

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(problemDetails));
        }
    }

    private static (int StatusCode, string Title, string Code) MapException(Exception ex) => ex switch
    {
        BusinessRuleException e => (StatusCodes.Status400BadRequest, "Business rule violation.", e.Code),
        NotFoundException e => (StatusCodes.Status404NotFound, "Resource not found.", e.Code),
        ConflictException e => (StatusCodes.Status409Conflict, "Conflict.", e.Code),
        ForbiddenException e => (StatusCodes.Status403Forbidden, "Forbidden.", e.Code),
        FaceServiceUnavailableException e => (StatusCodes.Status503ServiceUnavailable, "Face verification service unavailable.", e.Code),
        UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized.", "UNAUTHORIZED"),
        _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.", "INTERNAL_SERVER_ERROR")
    };

    private static string RfcSectionFor(int statusCode) => statusCode switch
    {
        400 => "15.5.1",
        401 => "15.5.2",
        403 => "15.5.4",
        404 => "15.5.5",
        409 => "15.5.10",
        500 => "15.6.1",
        503 => "15.6.4",
        _ => "15.5.1"
    };
}