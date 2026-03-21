using System.Net;
using System.Net.Http.Json;
using Xunit;
using FluentAssertions;

namespace MathLLMBackend.IntegrationTests;

[Collection("Integration Tests")]
public class UserControllerTests : BaseIntegrationTest
{
    public UserControllerTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetCurrentUser_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await Client.GetAsync("/api/user/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_WithAuth_ReturnsUserInfo()
    {
        var user = await CreateAndLoginUserAsync();
        var response = await AuthenticatedGetAsync("/api/user/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var userInfo = await response.Content.ReadFromJsonAsync<MathLLMBackend.Presentation.Dtos.Common.UserInfoDto>();
        userInfo.Should().NotBeNull();
        userInfo!.Email.Should().Be(user.Email);
        userInfo.Role.Should().NotBeNullOrEmpty();
    }
}
