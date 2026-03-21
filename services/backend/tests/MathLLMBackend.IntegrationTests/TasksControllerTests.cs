using System.Net;
using Xunit;
using FluentAssertions;
using MathLLMBackend.GeolinClient.Models;
using Moq;

namespace MathLLMBackend.IntegrationTests;

[Collection("Integration Tests")]
public class TasksControllerTests : BaseIntegrationTest
{
    public TasksControllerTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetProblems_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await Client.GetAsync("/api/tasks/problems");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProblems_WithAuth_ReturnsOk()
    {
        Factory.GeolinApiMock
            .Setup(x => x.GetProblemsInfo(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new ProblemPageResponse
            {
                Problems = new List<ProblemInfoResponse>
                {
                    new() { Name = "test", Hash = "hash", Description = "desc", ConditionRu = "condition" }
                },
                Number = 1
            });

        await CreateAndLoginAdminUserAsync();
        var response = await AuthenticatedGetAsync("/api/tasks/problems");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
