using FluentAssertions;
using MathLLMBackend.Core.Services.StatsService;
using MathLLMBackend.DataAccess.Contexts;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MathLLMBackend.Tests.Services;

public class StatsServiceTests
{
    private readonly AppDbContext _context;
    private readonly StatsService _service;

    public StatsServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new AppDbContext(options);
        var configMock = new Mock<IConfiguration>();
        var loggerMock = new Mock<ILogger<StatsService>>();
        _service = new StatsService(_context, configMock.Object, loggerMock.Object);
    }

    private ApplicationUser MakeUser(string id, string email = "u@test.com") =>
        new() { Id = id, UserName = email, Email = email, FirstName = "First", LastName = "Last", StudentGroup = "G1" };

    private async Task<Problem> SeedProblemAsync(TaskType taskType)
    {
        var problem = new Problem("sol", "stmt", "title") { TheoryLink = "link" };
        _context.Problems.Add(problem);
        var ptt = new ProblemTaskType(problem, taskType);
        _context.ProblemTaskTypes.Add(ptt);
        await _context.SaveChangesAsync();
        return problem;
    }

    private UserTask MakeUserTask(string userId, Problem problem, ProblemTaskType ptt, UserTaskStatus status) =>
        new()
        {
            ApplicationUserId = userId,
            ProblemId = problem.Id,
            ProblemHash = "hash",
            DisplayName = "Task",
            ProblemTaskType = ptt,
            TaskType = ptt.TaskType,
            Status = status
        };

    [Fact]
    public async Task GetUserStatsAsync_NoUsers_ReturnsEmptyList()
    {
        var result = await _service.GetUserStatsAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserStatsAsync_UsersWithTasksAndChats_ReturnsCorrectCounts()
    {
        const string userId = "user1";
        _context.Users.Add(MakeUser(userId, "user1@test.com"));

        var problem = await SeedProblemAsync(TaskType.Learning);
        var ptt = await _context.ProblemTaskTypes.FirstAsync();

        _context.UserTasks.Add(MakeUserTask(userId, problem, ptt, UserTaskStatus.Solved));
        _context.UserTasks.Add(MakeUserTask(userId, problem, ptt, UserTaskStatus.InProgress));
        _context.Chats.Add(new Chat { UserId = userId, Name = "Chat1", Type = ChatType.Chat });
        _context.Chats.Add(new Chat { UserId = userId, Name = "Chat2", Type = ChatType.ProblemSolver });
        await _context.SaveChangesAsync();

        var result = (await _service.GetUserStatsAsync()).ToList();

        result.Should().HaveCount(1);
        var stats = result[0];
        stats.UserId.Should().Be(userId);
        stats.SolvedCount.Should().Be(1);
        stats.InProgressCount.Should().Be(1);
        stats.NormalChatsCount.Should().Be(1);
    }

    [Fact]
    public async Task GetUserStatsAsync_UserWithNullEmail_ReturnsEmptyStringEmail()
    {
        var user = new ApplicationUser { Id = "user2", UserName = "nomail", Email = null, FirstName = "A", LastName = "B", StudentGroup = "G" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = (await _service.GetUserStatsAsync()).ToList();

        result.Should().HaveCount(1);
        result[0].Email.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserDetailsAsync_UserWithNoTasks_ReturnsEmptyTaskLists()
    {
        const string userId = "user1";

        var result = await _service.GetUserDetailsAsync(userId);

        result.SolvedTasks.Should().BeEmpty();
        result.InProgressTasks.Should().BeEmpty();
        result.Chats.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserDetailsAsync_UserWithSolvedAndInProgressTasks_ReturnsCorrectLists()
    {
        const string userId = "user1";
        var problem = await SeedProblemAsync(TaskType.Learning);
        var ptt = await _context.ProblemTaskTypes.FirstAsync();

        _context.UserTasks.Add(MakeUserTask(userId, problem, ptt, UserTaskStatus.Solved));
        _context.UserTasks.Add(MakeUserTask(userId, problem, ptt, UserTaskStatus.InProgress));
        await _context.SaveChangesAsync();

        var result = await _service.GetUserDetailsAsync(userId);

        result.SolvedTasks.Should().HaveCount(1);
        result.InProgressTasks.Should().HaveCount(1);
        result.SolvedTasks[0].TaskType.Should().Be(TaskType.Learning);
        result.InProgressTasks[0].TaskType.Should().Be(TaskType.Learning);
    }

    [Fact]
    public async Task GetUserDetailsAsync_UserWithNoChats_ReturnsEmptyChatList()
    {
        const string userId = "user1";
        var problem = await SeedProblemAsync(TaskType.Exam);
        var ptt = await _context.ProblemTaskTypes.FirstAsync();
        _context.UserTasks.Add(MakeUserTask(userId, problem, ptt, UserTaskStatus.Solved));
        await _context.SaveChangesAsync();

        var result = await _service.GetUserDetailsAsync(userId);

        result.Chats.Should().BeEmpty();
        result.SolvedTasks.Should().HaveCount(1);
    }
}
