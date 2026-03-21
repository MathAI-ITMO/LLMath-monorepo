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

        if (exception is AuthorizationException authEx)
        {
            response.StatusCode = (int)HttpStatusCode.Forbidden;
            errorResponse.Status = response.StatusCode;
            errorResponse.Title = "Forbidden";
            errorResponse.Detail = authEx.Message;
            _logger.LogInformation("Authorization exception: {Message}", authEx.Message);
        }
        else if (exception is NotFoundException notFoundEx)
        {
            response.StatusCode = (int)HttpStatusCode.NotFound;
            errorResponse.Status = response.StatusCode;
            errorResponse.Title = "Not Found";
            errorResponse.Detail = notFoundEx.Message;
            _logger.LogInformation("Resource not found: {Message}", notFoundEx.Message);
        }
        else if (exception is ArgumentException or ArgumentNullException)
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            errorResponse.Status = response.StatusCode;
            errorResponse.Title = "Bad Request";
            errorResponse.Detail = exception.Message;
            _logger.LogWarning("Invalid argument: {Message}", exception.Message);
        }
        else if (exception is InvalidOperationException invalidOpEx)
        {
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            errorResponse.Status = response.StatusCode;
            errorResponse.Title = "Invalid Operation";
            errorResponse.Detail = invalidOpEx.Message;
            _logger.LogWarning("Invalid operation: {Message}", invalidOpEx.Message);
        }
        else if (exception is UnauthorizedAccessException)
        {
            response.StatusCode = (int)HttpStatusCode.Forbidden;
            errorResponse.Status = response.StatusCode;
            errorResponse.Title = "Forbidden";
            errorResponse.Detail = exception.Message;
            _logger.LogInformation("Unauthorized access: {Message}", exception.Message);
        }
        else
        {
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            errorResponse.Status = response.StatusCode;
            errorResponse.Title = "Internal Server Error";
            errorResponse.Detail = _environment.IsDevelopment() 
                ? exception.ToString() 
                : "An error occurred while processing your request.";
            _logger.LogError(exception, "Unhandled exception occurred");
        }

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
