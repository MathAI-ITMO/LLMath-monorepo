using System.Net;
using System.Text.Json;
using MathLLMBackend.Domain.Exceptions;

namespace MathLLMBackend.Presentation.Middlewares;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ErrorResponse();

        var (statusCode, title, detail, logAction) = exception switch
        {
            AuthorizationException ex => (
                HttpStatusCode.Forbidden, "Forbidden", ex.Message,
                (Action)(() => _logger.LogInformation("Authorization exception: {Message}", ex.Message))),

            NotFoundException ex => (
                HttpStatusCode.NotFound, "Not Found", ex.Message,
                () => _logger.LogInformation("Resource not found: {Message}", ex.Message)),

            ArgumentException or ArgumentNullException => (
                HttpStatusCode.BadRequest, "Bad Request", exception.Message,
                () => _logger.LogWarning("Invalid argument: {Message}", exception.Message)),

            InvalidOperationException ex => (
                HttpStatusCode.BadRequest, "Invalid Operation", ex.Message,
                () => _logger.LogWarning("Invalid operation: {Message}", ex.Message)),

            UnauthorizedAccessException => (
                HttpStatusCode.Forbidden, "Forbidden", exception.Message,
                () => _logger.LogInformation("Unauthorized access: {Message}", exception.Message)),

            _ => (
                HttpStatusCode.InternalServerError, "Internal Server Error",
                _environment.IsDevelopment() ? exception.ToString() : "An error occurred while processing your request.",
                () => _logger.LogError(exception, "Unhandled exception occurred"))
        };

        response.StatusCode = (int)statusCode;
        errorResponse.Status = response.StatusCode;
        errorResponse.Title = title;
        errorResponse.Detail = detail;
        logAction();

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(errorResponse, jsonOptions);
        await response.WriteAsync(json);
    }

    private class ErrorResponse
    {
        public int Status { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
    }
}
