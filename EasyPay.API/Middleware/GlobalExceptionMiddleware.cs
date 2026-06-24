using EasyPay.Core.DTOs;
using EasyPay.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EasyPay.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next   = next;
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
            _logger.LogError(ex, "Unhandled exception for request {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            ValidationException ve       => (400, ve.Message,          ve.Errors),
            NotFoundException nfe         => (404, nfe.Message,         null),
            UnauthorizedException uae     => (401, uae.Message,         null),
            ForbiddenException fe         => (403, fe.Message,          null),
            ConflictException ce          => (409, ce.Message,          null),
            BusinessRuleException bre     => (422, bre.Message,         null),
            EasyPayException epe          => (epe.StatusCode, epe.Message, null),
            _                            => (500, "An unexpected error occurred. Please try again later.", null)
        };

        context.Response.StatusCode = statusCode;

        var response = new ApiResponse<object>
        {
            Success   = false,
            Message   = message,
            Errors    = errors,
            Timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
