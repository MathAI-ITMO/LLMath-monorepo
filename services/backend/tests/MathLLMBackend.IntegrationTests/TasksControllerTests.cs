using System.Net;
using Xunit;
using FluentAssertions;
using MathLLMBackend.GeolinClient.Models;
using Microsoft.AspNetCore.Http;
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
        var response = await Client.GetAsync("/api/tasks/problem/test");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProblems_WithAuth_ReturnsOk()
    {
        Factory.GeolinApiMock
            .Setup(x => x.GetProblemsInfo(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProblemPageResponse
            {
                Problems = new List<ProblemInfoResponse>
                {
                    new() { Name = "test", Hash = "hash", Description = "desc", ConditionRu = "condition" }
                },
                Number = 1
            });
        Factory.GeolinApiMock
            .Setup(x => x.GetProblemCondition(It.IsAny<ProblemConditionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProblemConditionResponse { Condition = "Test condition", ProblemParams = "{}" });

        await CreateAndLoginAdminUserAsync();
        var response = await AuthenticatedGetAsync("/api/tasks/problem/test");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
