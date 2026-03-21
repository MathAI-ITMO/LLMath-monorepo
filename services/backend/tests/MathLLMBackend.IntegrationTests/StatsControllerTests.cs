using System.Net;
using Xunit;
using FluentAssertions;
using System.Net.Http.Json;

namespace MathLLMBackend.IntegrationTests;

[Collection("Integration Tests")]
public class StatsControllerTests : BaseIntegrationTest
{
    public StatsControllerTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetTaskModeTitles_ReturnsOk()
    {
        await CreateAndLoginAdminUserAsync();
        var response = await AuthenticatedGetAsync("/api/stats/task-mode-titles");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetUserStats_ReturnsOk()
    {
        await CreateAndLoginAdminUserAsync();
        var response = await AuthenticatedGetAsync("/api/stats/user-stats");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetUserDetails_WithValidUserId_ReturnsOk()
    {
        var adminUser = await CreateAndLoginAdminUserAsync();
        var regularUser = await Factory.CreateTestUserAsync("regular@example.com", "Test123!@#");
        var response = await AuthenticatedClient!.GetAsync($"/api/stats/user-details/{regularUser.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetUserDetails_AsRegularUser_ReturnsForbidden()
    {
        await CreateAndLoginUserAsync();
        var response = await AuthenticatedClient!.GetAsync("/api/stats/user-details/some-user-id");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
