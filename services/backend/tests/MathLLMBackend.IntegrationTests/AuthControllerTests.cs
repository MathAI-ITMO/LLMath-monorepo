using System.Net;
using Xunit;
using System.Net.Http.Json;
using FluentAssertions;
using MathLLMBackend.Domain.Constants;
using MathLLMBackend.Presentation.Dtos.Auth;
using MathLLMBackend.Presentation.Dtos.Common;

namespace MathLLMBackend.IntegrationTests;

[Collection("Integration Tests")]
public class AuthControllerTests : BaseIntegrationTest
{
    public AuthControllerTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsOk()
    {
        var registerDto = new RegisterDto(
            FirstName: "John",
            LastName: "Doe",
            StudentGroup: "Group1",
            Email: $"john.doe.{Guid.NewGuid()}@example.com",
            Password: "Test123!@#"
        );

        var response = await Client.PostAsJsonAsync("/api/auth/register", registerDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var userInfo = await response.Content.ReadFromJsonAsync<UserInfoDto>();
        userInfo.Should().NotBeNull();
        userInfo!.Email.Should().Be(registerDto.Email);
        userInfo.Role.Should().Be(Role.User);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        var registerDto = new RegisterDto(
            FirstName: "John",
            LastName: "Doe",
            StudentGroup: "Group1",
            Email: "duplicate@example.com",
            Password: "Test123!@#"
        );

        await Client.PostAsJsonAsync("/api/auth/register", registerDto);
        var response = await Client.PostAsJsonAsync("/api/auth/register", registerDto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithInvalidPassword_ReturnsBadRequest()
    {
        var registerDto = new RegisterDto(
            FirstName: "John",
            LastName: "Doe",
            StudentGroup: "Group1",
            Email: "invalid@example.com",
            Password: "123"
        );

        var response = await Client.PostAsJsonAsync("/api/auth/register", registerDto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithInvalidModelState_ReturnsBadRequest()
    {
        var invalidDto = new { Email = "not-an-email" };
        var response = await Client.PostAsJsonAsync("/api/auth/register", invalidDto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
