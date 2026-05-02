using System.Net;
using System.Text.Json;
using FluentAssertions;
using MathLLMBackend.Domain.Exceptions;
using MathLLMBackend.Presentation.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MathLLMBackend.IntegrationTests;

public class GlobalExceptionHandlerMiddlewareTests
{
    private readonly Mock<ILogger<GlobalExceptionHandlerMiddleware>> _loggerMock = new();

    private static DefaultHttpContext CreateContext()
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();
        return ctx;
    }

    private static IHostEnvironment MakeEnv(string name)
    {
        var mock = new Mock<IHostEnvironment>();
        mock.Setup(e => e.EnvironmentName).Returns(name);
        return mock.Object;
    }

    private async Task<(int Status, string Title, string Detail)> InvokeAsync(Exception ex, string envName = "Production")
    {
        var ctx = CreateContext();
        RequestDelegate next = _ => Task.FromException(ex);
        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object, MakeEnv(envName));

        await middleware.InvokeAsync(ctx);

        ctx.Response.Body.Position = 0;
        var body = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        return (
            root.GetProperty("status").GetInt32(),
            root.GetProperty("title").GetString()!,
            root.GetProperty("detail").GetString()!
        );
    }

    [Fact]
    public async Task AuthorizationException_Returns403Forbidden()
    {
        var (status, title, _) = await InvokeAsync(new AuthorizationException("access denied"));

        status.Should().Be((int)HttpStatusCode.Forbidden);
        title.Should().Be("Forbidden");
    }

    [Fact]
    public async Task NotFoundException_Returns404NotFound()
    {
        var (status, title, _) = await InvokeAsync(new NotFoundException("not found"));

        status.Should().Be((int)HttpStatusCode.NotFound);
        title.Should().Be("Not Found");
    }

    [Fact]
    public async Task ArgumentException_Returns400BadRequest()
    {
        var (status, title, _) = await InvokeAsync(new ArgumentException("bad arg"));

        status.Should().Be((int)HttpStatusCode.BadRequest);
        title.Should().Be("Bad Request");
    }

    [Fact]
    public async Task InvalidOperationException_Returns400BadRequest()
    {
        var (status, title, _) = await InvokeAsync(new InvalidOperationException("invalid op"));

        status.Should().Be((int)HttpStatusCode.BadRequest);
        title.Should().Be("Invalid Operation");
    }

    [Fact]
    public async Task UnauthorizedAccessException_Returns403Forbidden()
    {
        var (status, title, _) = await InvokeAsync(new UnauthorizedAccessException("forbidden"));

        status.Should().Be((int)HttpStatusCode.Forbidden);
        title.Should().Be("Forbidden");
    }

    [Fact]
    public async Task UnhandledException_ProductionEnv_Returns500WithGenericMessage()
    {
        var (status, _, detail) = await InvokeAsync(new Exception("internal"), "Production");

        status.Should().Be((int)HttpStatusCode.InternalServerError);
        detail.Should().Be("An error occurred while processing your request.");
    }

    [Fact]
    public async Task UnhandledException_DevelopmentEnv_Returns500WithExceptionDetails()
    {
        var ex = new Exception("dev error");
        var (status, _, detail) = await InvokeAsync(ex, "Development");

        status.Should().Be((int)HttpStatusCode.InternalServerError);
        detail.Should().Contain("dev error");
    }

    [Fact]
    public async Task AnyException_ResponseBodyIsValidCamelCaseJson()
    {
        var ctx = CreateContext();
        RequestDelegate next = _ => Task.FromException(new ArgumentException("test"));
        var middleware = new GlobalExceptionHandlerMiddleware(next, _loggerMock.Object, MakeEnv("Production"));

        await middleware.InvokeAsync(ctx);

        ctx.Response.ContentType.Should().Be("application/json");
        ctx.Response.Body.Position = 0;
        var body = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.TryGetProperty("status", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("title", out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("detail", out _).Should().BeTrue();
    }
}
