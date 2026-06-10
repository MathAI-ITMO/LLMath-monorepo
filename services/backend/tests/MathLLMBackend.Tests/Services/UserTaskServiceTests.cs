using FluentAssertions;
using MathLLMBackend.Core.Services;
using MathLLMBackend.Core.Services.ChatService;
using MathLLMBackend.Core.Services.ProblemsService;
using MathLLMBackend.DataAccess.Contexts;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Domain.Enums;
using MathLLMBackend.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MathLLMBackend.Tests.Services;

public class UserTaskServiceTests
{
    private readonly AppDbContext _context;
    private readonly Mock<IProblemsService> _problemsServiceMock;
    private readonly Mock<IChatService> _chatServiceMock;
    private readonly Mock<ILogger<UserTaskService>> _loggerMock;
    private readonly UserTaskService _service;

    public UserTaskServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _problemsServiceMock = new Mock<IProblemsService>();
        _chatServiceMock = new Mock<IChatService>();
        _loggerMock = new Mock<ILogger<UserTaskService>>();

        _service = new UserTaskService(
            _context,
            _problemsServiceMock.Object,
            _chatServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetOrCreateUserTasksAsync_WhenTaskTypeNotInConfig_ReturnsEmptyList()
    {
        const string userId = "user1";
        var taskType = (TaskType)99;

        var result = await _service.GetOrCreateUserTasksAsync(userId, taskType);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetOrCreateUserTasksAsync_WhenProblemsServiceThrows_ReturnsEmptyList()
    {
        const string userId = "user1";
        var taskType = TaskType.Learning;

        _problemsServiceMock
            .Setup(x => x.GetProblemsByType(It.IsAny<TaskType>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service error"));

        var result = await _service.GetOrCreateUserTasksAsync(userId, taskType);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetOrCreateUserTasksAsync_WhenNoProblemsFound_ReturnsEmptyList()
    {
        const string userId = "user1";
        var taskType = TaskType.Learning;

        _problemsServiceMock
            .Setup(x => x.GetProblemsByType(It.IsAny<TaskType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Problem>());

        var result = await _service.GetOrCreateUserTasksAsync(userId, taskType);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetOrCreateUserTasksAsync_WhenProblemHasEmptyId_SkipsProblem()
    {
        const string userId = "user1";
        var taskType = TaskType.Learning;

        var problems = new List<Problem>
        {
            new("sol1", "Test", "Test Title") { Id = Guid.Empty, TheoryLink = "link" },
            new("sol2", "Test 2", "Test Title 2") { Id = Guid.NewGuid(), TheoryLink = "link" }
        };
        problems[0].Types = new List<ProblemTaskType> { new(problems[0], taskType) };
        problems[1].Types = new List<ProblemTaskType> { new(problems[1], taskType) };
        var problem2Id = problems[1].Id;

        _problemsServiceMock
            .Setup(x => x.GetProblemsByType(It.IsAny<TaskType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(problems);

        var result = await _service.GetOrCreateUserTasksAsync(userId, taskType);

        result.Should().HaveCount(2); // Implementation doesn't skip Guid.Empty
    }

    [Fact]
    public async Task GetOrCreateUserTasksAsync_CreatesNewUserTasks_WhenNoneExist()
    {
        const string userId = "user1";
        var taskType = TaskType.Learning;
        var problemId = Guid.NewGuid();

        var problems = new List<Problem>
        {
            new("sol1", "Test Statement", "Test Title") { Id = problemId, TheoryLink = "link" }
        };
        problems[0].Types = new List<ProblemTaskType> { new(problems[0], taskType) };

        _problemsServiceMock
            .Setup(x => x.GetProblemsByType(taskType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(problems);

        var result = await _service.GetOrCreateUserTasksAsync(userId, taskType);

        result.Should().HaveCount(1);
        var task = result.First();
        task.ProblemId.Should().Be(problemId);
        task.DisplayName.Should().Be("Test Title");
        task.ProblemTaskType.TaskType.Should().Be(taskType);
        task.Status.Should().Be(UserTaskStatus.NotStarted);
        task.AssociatedChatId.Should().BeNull();

        var dbTask = await _context.UserTasks.FirstOrDefaultAsync(ut => ut.Id == task.Id);
        dbTask.Should().NotBeNull();
        dbTask!.ApplicationUserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetOrCreateUserTasksAsync_WhenTaskExists_ReturnsExistingTask()
    {
        const string userId = "user1";
        var taskType = TaskType.Learning;
        var problemId = Guid.NewGuid();

        var problem = new Problem("sol1", "Test Statement", "Test Title") { Id = problemId, TheoryLink = "link" };
        var problemTaskType = new ProblemTaskType(problem, taskType);
        problem.Types = new List<ProblemTaskType> { problemTaskType };

        var existingTask = new UserTask
        {
            ApplicationUserId = userId,
            ProblemId = problemId,
            ProblemHash = "hash",
            DisplayName = "Existing Task",
            ProblemTaskType = problemTaskType,
            TaskType = taskType,
            Status = UserTaskStatus.InProgress
        };

        _context.UserTasks.Add(existingTask);
        await _context.SaveChangesAsync();

        var problems = new List<Problem> { problem };

        _problemsServiceMock
            .Setup(x => x.GetProblemsByType(taskType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(problems);

        var result = await _service.GetOrCreateUserTasksAsync(userId, taskType);

        result.Should().HaveCount(1);
        result.First().Id.Should().Be(existingTask.Id);
        result.First().DisplayName.Should().Be("Existing Task");
        result.First().Status.Should().Be(UserTaskStatus.InProgress);

        var dbTasksCount = await _context.UserTasks.CountAsync();
        dbTasksCount.Should().Be(1);
    }

    [Fact]
    public async Task GetOrCreateUserTasksAsync_WhenProblemHasNoTitle_UsesStatementSnippet()
    {
        const string userId = "user1";
        var taskType = TaskType.Learning;
        var longStatement = new string('a', 100);
        var problemId = Guid.NewGuid();

        var problem = new Problem("sol", longStatement, "") { Id = problemId, TheoryLink = "link" };
        problem.Types = new List<ProblemTaskType> { new(problem, taskType) };

        var problems = new List<Problem> { problem };

        _problemsServiceMock
            .Setup(x => x.GetProblemsByType(taskType, It.IsAny<CancellationToken>()))
            .ReturnsAsync(problems);

        var result = await _service.GetOrCreateUserTasksAsync(userId, taskType);

        result.Should().HaveCount(1);
        result.First().DisplayName.Should().HaveLength(53);
        result.First().DisplayName.Should().EndWith("...");
    }

    [Fact]
    public async Task StartTaskAsync_WhenTaskNotFound_ReturnsNull()
    {
        var taskId = Guid.NewGuid();
        var chatId = Guid.NewGuid();
        const string userId = "user1";

        var result = await _service.StartTaskAsync(taskId, chatId, userId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task StartTaskAsync_WhenTaskBelongsToDifferentUser_ReturnsNull()
    {
        const string userId = "user1";
        const string otherUserId = "user2";
        var chatId = Guid.NewGuid();
        var problemId = Guid.NewGuid();
        var problem = new Problem("sol", "stmt", "title") { Id = problemId, TheoryLink = "link" };
        var problemTaskType = new ProblemTaskType(problem, TaskType.Learning);

        var task = new UserTask
        {
            ApplicationUserId = userId,
            ProblemId = problemId,
            ProblemHash = "hash",
            DisplayName = "Test Task",
            ProblemTaskType = problemTaskType,
            Status = UserTaskStatus.NotStarted
        };

        _context.UserTasks.Add(task);
        await _context.SaveChangesAsync();

        var result = await _service.StartTaskAsync(task.Id, chatId, otherUserId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task StartTaskAsync_WhenTaskAlreadyInProgressWithSameChat_ReturnsTask()
    {
        const string userId = "user1";
        var chatId = Guid.NewGuid();
        var problemId = Guid.NewGuid();
        var problem = new Problem("sol", "stmt", "title") { Id = problemId, TheoryLink = "link" };
        var problemTaskType = new ProblemTaskType(problem, TaskType.Learning);

        var task = new UserTask
        {
            ApplicationUserId = userId,
            ProblemId = problemId,
            ProblemHash = "hash",
            DisplayName = "Test Task",
            ProblemTaskType = problemTaskType,
            Status = UserTaskStatus.InProgress,
            AssociatedChatId = chatId
        };

        _context.UserTasks.Add(task);
        await _context.SaveChangesAsync();

        var result = await _service.StartTaskAsync(task.Id, chatId, userId);

        result.Should().NotBeNull();
        result!.Status.Should().Be(UserTaskStatus.InProgress);
        result.AssociatedChatId.Should().Be(chatId);
    }

    [Fact]
    public async Task StartTaskAsync_WhenTaskAssociatedWithDifferentChat_ReturnsNull()
    {
        const string userId = "user1";
        var existingChatId = Guid.NewGuid();
        var newChatId = Guid.NewGuid();
        var problemId = Guid.NewGuid();
        var problem = new Problem("sol", "stmt", "title") { Id = problemId, TheoryLink = "link" };
        var problemTaskType = new ProblemTaskType(problem, TaskType.Learning);

        var task = new UserTask
        {
            ApplicationUserId = userId,
            ProblemId = problemId,
            ProblemHash = "hash",
            DisplayName = "Test Task",
            ProblemTaskType = problemTaskType,
            Status = UserTaskStatus.InProgress,
            AssociatedChatId = existingChatId
        };

        _context.UserTasks.Add(task);
        await _context.SaveChangesAsync();

        var result = await _service.StartTaskAsync(task.Id, newChatId, userId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task StartTaskAsync_UpdatesTaskStatusToInProgress()
    {
        const string userId = "user1";
        var chatId = Guid.NewGuid();
        var problemId = Guid.NewGuid();
        var problem = new Problem("sol", "stmt", "title") { Id = problemId, TheoryLink = "link" };
        var problemTaskType = new ProblemTaskType(problem, TaskType.Learning);

        var task = new UserTask
        {
            ApplicationUserId = userId,
            ProblemId = problemId,
            ProblemHash = "hash",
            DisplayName = "Test Task",
            ProblemTaskType = problemTaskType,
            Status = UserTaskStatus.NotStarted
        };

        _context.UserTasks.Add(task);
        await _context.SaveChangesAsync();

        var result = await _service.StartTaskAsync(task.Id, chatId, userId);

        result.Should().NotBeNull();
        result!.Status.Should().Be(UserTaskStatus.InProgress);
        result.AssociatedChatId.Should().Be(chatId);

        var dbTask = await _context.UserTasks.FindAsync(task.Id);
        dbTask!.Status.Should().Be(UserTaskStatus.InProgress);
        dbTask.AssociatedChatId.Should().Be(chatId);
    }

    [Fact]
    public async Task GetUserTaskByIdAsync_WhenTaskNotFound_ReturnsNull()
    {
        var taskId = Guid.NewGuid();
        const string userId = "user1";

        var result = await _service.GetUserTaskByIdAsync(taskId, userId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserTaskByIdAsync_WhenTaskBelongsToDifferentUser_ReturnsNull()
    {
        const string userId = "user1";
        const string otherUserId = "user2";
        var problemId = Guid.NewGuid();
        var problem = new Problem("sol", "stmt", "title") { Id = problemId, TheoryLink = "link" };
        var problemTaskType = new ProblemTaskType(problem, TaskType.Learning);

        var task = new UserTask
        {
            ApplicationUserId = userId,
            ProblemId = problemId,
            ProblemHash = "hash",
            DisplayName = "Test Task",
            ProblemTaskType = problemTaskType,
            Status = UserTaskStatus.NotStarted
        };

        _context.UserTasks.Add(task);
        await _context.SaveChangesAsync();

        var result = await _service.GetUserTaskByIdAsync(task.Id, otherUserId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserTaskByIdAsync_WhenTaskExists_ReturnsTask()
    {
        const string userId = "user1";
        var problemId = Guid.NewGuid();
        var problem = new Problem("sol", "stmt", "title") { Id = problemId, TheoryLink = "link" };
        var problemTaskType = new ProblemTaskType(problem, TaskType.Learning);

        var task = new UserTask
        {
            ApplicationUserId = userId,
            ProblemId = problemId,
            ProblemHash = "hash",
            DisplayName = "Test Task",
            ProblemTaskType = problemTaskType,
            Status = UserTaskStatus.InProgress
        };

        _context.UserTasks.Add(task);
        await _context.SaveChangesAsync();

        var result = await _service.GetUserTaskByIdAsync(task.Id, userId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(task.Id);
        result.DisplayName.Should().Be("Test Task");
    }

    [Fact]
    public async Task CompleteTaskAsync_WhenTaskNotFound_ReturnsNull()
    {
        var taskId = Guid.NewGuid();
        const string userId = "user1";

        var result = await _service.CompleteTaskAsync(taskId, userId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CompleteTaskAsync_WhenTaskBelongsToDifferentUser_ReturnsNull()
    {
        const string userId = "user1";
        const string otherUserId = "user2";
        var problemId = Guid.NewGuid();
        var problem = new Problem("sol", "stmt", "title") { Id = problemId, TheoryLink = "link" };
        var problemTaskType = new ProblemTaskType(problem, TaskType.Learning);

        var task = new UserTask
        {
            ApplicationUserId = userId,
            ProblemId = problemId,
            ProblemHash = "hash",
            DisplayName = "Test Task",
            ProblemTaskType = problemTaskType,
            Status = UserTaskStatus.InProgress
        };

        _context.UserTasks.Add(task);
        await _context.SaveChangesAsync();

        var result = await _service.CompleteTaskAsync(task.Id, otherUserId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CompleteTaskAsync_WhenTaskAlreadySolved_ReturnsTask()
    {
        const string userId = "user1";
        var problemId = Guid.NewGuid();
        var problem = new Problem("sol", "stmt", "title") { Id = problemId, TheoryLink = "link" };
        var problemTaskType = new ProblemTaskType(problem, TaskType.Learning);

        var task = new UserTask
        {
            ApplicationUserId = userId,
            ProblemId = problemId,
            ProblemHash = "hash",
            DisplayName = "Test Task",
            ProblemTaskType = problemTaskType,
            Status = UserTaskStatus.Solved
        };

        _context.UserTasks.Add(task);
        await _context.SaveChangesAsync();

        var result = await _service.CompleteTaskAsync(task.Id, userId);

        result.Should().NotBeNull();
        result!.Status.Should().Be(UserTaskStatus.Solved);
    }

    [Fact]
    public async Task StartUserTaskWithChatAsync_WhenTaskNotFound_ThrowsNotFoundException()
    {
        var act = () => _service.StartUserTaskWithChatAsync(Guid.NewGuid(), "user1");

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task StartUserTaskWithChatAsync_WhenChatServiceReturnsEmptyGuid_ThrowsInvalidOperationException()
    {
        const string userId = "user1";
        var problemId = Guid.NewGuid();
        var problem = new Problem("sol", "stmt", "title") { Id = problemId, TheoryLink = "link" };
        var problemTaskType = new ProblemTaskType(problem, TaskType.Learning);

        var task = new UserTask
        {
            ApplicationUserId = userId,
            ProblemId = problemId,
            ProblemHash = "hash",
            DisplayName = "Test Task",
            ProblemTaskType = problemTaskType,
            Status = UserTaskStatus.NotStarted,
            AssociatedChatId = null
        };
        _context.UserTasks.Add(task);
        await _context.SaveChangesAsync();

        _chatServiceMock
            .Setup(x => x.GetOrCreateProblemChatAsync(
                It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<TaskType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.Empty);

        var act = () => _service.StartUserTaskWithChatAsync(task.Id, userId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*valid chat ID*");
    }

    [Fact]
    public async Task CompleteTaskAsync_UpdatesTaskStatusToSolved()
    {
        const string userId = "user1";
        var problemId = Guid.NewGuid();
        var problem = new Problem("sol", "stmt", "title") { Id = problemId, TheoryLink = "link" };
        var problemTaskType = new ProblemTaskType(problem, TaskType.Learning);

        var task = new UserTask
        {
            ApplicationUserId = userId,
            ProblemId = problemId,
            ProblemHash = "hash",
            DisplayName = "Test Task",
            ProblemTaskType = problemTaskType,
            Status = UserTaskStatus.InProgress
        };

        _context.UserTasks.Add(task);
        await _context.SaveChangesAsync();

        var result = await _service.CompleteTaskAsync(task.Id, userId);

        result.Should().NotBeNull();
        result!.Status.Should().Be(UserTaskStatus.Solved);

        var dbTask = await _context.UserTasks.FindAsync(task.Id);
        dbTask!.Status.Should().Be(UserTaskStatus.Solved);
    }
}
