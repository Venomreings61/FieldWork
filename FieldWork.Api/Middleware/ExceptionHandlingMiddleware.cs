using System.Text.Json;
using FieldWork.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

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
                if (statusCode == StatusCodes.Status500InternalServerError)
                {
                    _logger.LogError(ex, "Unhandled exception.");
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
                Type = $"https://tools.ietf.org/html/rfc9110#section-15.5.{SectionFor(statusCode)}",
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
        UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized.", "UNAUTHORIZED"),
        _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.", "INTERNAL_SERVER_ERROR")
    };

    private static int SectionFor(int statusCode) => statusCode switch
    {
        400 => 1,
        401 => 2,
        403 => 4,
        404 => 5,
        409 => 10,
        _ => 1
    };
}