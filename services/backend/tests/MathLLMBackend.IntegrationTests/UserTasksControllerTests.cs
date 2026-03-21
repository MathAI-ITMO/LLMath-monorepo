using System.Net;
using Xunit;
using FluentAssertions;
using System.Net.Http.Json;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Linq;

namespace MathLLMBackend.IntegrationTests;

[Collection("Integration Tests")]
public class UserTasksControllerTests : BaseIntegrationTest
{
    public UserTasksControllerTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetUserTasks_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await Client.GetAsync("/api/usertasks");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserTasks_WithAuth_ReturnsOk()
    {
        await CreateAndLoginUserAsync();
        var response = await AuthenticatedGetAsync("/api/usertasks");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task StartUserTask_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await Client.PostAsync("/api/usertasks/00000000-0000-0000-0000-000000000000/start", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task StartUserTask_WithNonExistentTask_ReturnsNotFound()
    {
        await CreateAndLoginUserAsync();
        var response = await AuthenticatedPostAsync("/api/usertasks/00000000-0000-0000-0000-000000000000/start");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CompleteUserTask_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await Client.PostAsync("/api/usertasks/00000000-0000-0000-0000-000000000000/complete", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CompleteUserTask_WithNonExistentTask_ReturnsNotFound()
    {
        await CreateAndLoginUserAsync();
        var response = await AuthenticatedPostAsync("/api/usertasks/00000000-0000-0000-0000-000000000000/complete");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task StartUserTask_WithValidTask_ReturnsOk()
    {
        await CreateAndLoginUserAsync();

        Guid taskId;
        using (var scope = Factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<MathLLMBackend.DataAccess.Contexts.AppDbContext>();
            var problem = new Problem("sol", "stmt", "title") { Id = Guid.NewGuid(), TheoryLink = "link" };
            var problemTaskType = new ProblemTaskType(problem, TaskType.Learning);
            problem.Types = new List<ProblemTaskType> { problemTaskType };
            dbContext.Problems.Add(problem);
            
            var userTask = new UserTask
            {
                ApplicationUserId = TestUser!.Id,
                ProblemId = problem.Id,
                ProblemHash = problem.Id.ToString(),
                DisplayName = "Test Task",
                ProblemTaskType = problemTaskType,
                TaskType = TaskType.Learning,
                Status = UserTaskStatus.NotStarted
            };
            dbContext.UserTasks.Add(userTask);
            await dbContext.SaveChangesAsync();
            taskId = userTask.Id;
        }

        Factory.LlmServiceMock
            .Setup(x => x.GenerateNextMessageAsync(It.IsAny<List<MathLLMBackend.Domain.Entities.Message>>(), It.IsAny<TaskType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Test response");

        var response = await AuthenticatedPostAsync($"/api/usertasks/{taskId}/start");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CompleteUserTask_WithValidTask_ReturnsOk()
    {
        await CreateAndLoginUserAsync();
        
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MathLLMBackend.DataAccess.Contexts.AppDbContext>();
        
        var problem = new Problem("sol", "stmt", "title") { Id = Guid.NewGuid(), TheoryLink = "link" };
        var problemTaskType = new ProblemTaskType(problem, TaskType.Learning);
        problem.Types = new List<ProblemTaskType> { problemTaskType };
        dbContext.Problems.Add(problem);

        var userTask = new MathLLMBackend.Domain.Entities.UserTask
        {
            ApplicationUserId = TestUser!.Id,
            ProblemId = problem.Id,
            ProblemHash = "test-problem-id",
            DisplayName = "Test Task",
            ProblemTaskType = problemTaskType,
            TaskType = TaskType.Learning,
            Status = MathLLMBackend.Domain.Enums.UserTaskStatus.InProgress
        };
        dbContext.UserTasks.Add(userTask);
        await dbContext.SaveChangesAsync();

        var response = await AuthenticatedPostAsync($"/api/usertasks/{userTask.Id}/complete");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
