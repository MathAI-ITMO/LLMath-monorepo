using FluentAssertions;
using MathLLMBackend.Core.Services.ChatService;
using MathLLMBackend.Core.Services.LlmService;
using MathLLMBackend.Core.Services.PromptService;
using MathLLMBackend.DataAccess.Contexts;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MathLLMBackend.Tests.Services;

public class ChatServiceTests
{
    private readonly AppDbContext _context;
    private readonly Mock<ILlmService> _llmServiceMock;
    private readonly Mock<IPromptService> _promptServiceMock;
    private readonly Mock<ILogger<ChatService>> _loggerMock;
    private readonly ChatService _service;

    public ChatServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new AppDbContext(options);
        _llmServiceMock = new Mock<ILlmService>();
        _promptServiceMock = new Mock<IPromptService>();
        _loggerMock = new Mock<ILogger<ChatService>>();

        _service = new ChatService(_context, _llmServiceMock.Object, _promptServiceMock.Object, _loggerMock.Object);
    }

    private async Task<Chat> SeedChatAsync(string userId, ChatType chatType = ChatType.Chat)
    {
        var chat = new Chat { Name = "Test Chat", UserId = userId, Type = chatType };
        _context.Chats.Add(chat);
        await _context.SaveChangesAsync();
        return chat;
    }

    private async Task SeedSystemMessageAsync(Chat chat, string text)
    {
        var msg = new Message { ChatId = chat.Id, Text = text, MessageType = MessageType.System, IsSystemPrompt = true };
        _context.Messages.Add(msg);
        await _context.SaveChangesAsync();
    }

    private void SetupPromptMocksDefault()
    {
        _promptServiceMock.Setup(x => x.GetSystemPromptByTaskType(It.IsAny<TaskType>())).Returns("system prompt");
        _promptServiceMock.Setup(x => x.GetTutorSolutionPrompt(It.IsAny<string>())).Returns("solution prompt");
        _promptServiceMock.Setup(x => x.GetInitialPromptByTaskType(It.IsAny<TaskType>(), It.IsAny<string>(), It.IsAny<string>())).Returns("initial prompt");
        _promptServiceMock.Setup(x => x.GetDefaultSystemPrompt()).Returns("default system prompt");
        _promptServiceMock.Setup(x => x.GetLearningSystemPrompt()).Returns("learning prompt");
        _promptServiceMock.Setup(x => x.GetGuidedSystemPrompt()).Returns("guided prompt");
        _promptServiceMock.Setup(x => x.GetExamSystemPrompt()).Returns("exam prompt");
    }

    // DetermineTaskTypeFromSystemPrompt — tested via CreateMessageForUser on ProblemSolver chats

    [Fact]
    public async Task CreateMessageForUser_ProblemSolverNoSystemMsg_PassesDefaultTaskTypeToLlm()
    {
        const string userId = "user1";
        var chat = await SeedChatAsync(userId, ChatType.ProblemSolver);
        _context.Messages.Add(new Message { ChatId = chat.Id, Text = "hi", MessageType = MessageType.User, IsSystemPrompt = false });
        await _context.SaveChangesAsync();

        SetupPromptMocksDefault();
        TaskType captured = TaskType.Learning;
        _llmServiceMock
            .Setup(x => x.GenerateNextMessageAsync(It.IsAny<List<Message>>(), It.IsAny<TaskType>(), It.IsAny<CancellationToken>()))
            .Callback<List<Message>, TaskType, CancellationToken>((_, tt, _) => captured = tt)
            .ReturnsAsync("bot");

        await _service.CreateMessageForUser(chat.Id, userId, "text", CancellationToken.None);

        captured.Should().Be(TaskType.Default);
    }

    [Fact]
    public async Task CreateMessageForUser_ProblemSolverWithLearningPrompt_PassesLearningTaskTypeToLlm()
    {
        const string userId = "user1";
        const string learningPrompt = "learning system prompt text";
        var chat = await SeedChatAsync(userId, ChatType.ProblemSolver);
        await SeedSystemMessageAsync(chat, learningPrompt);

        _promptServiceMock.Setup(x => x.GetLearningSystemPrompt()).Returns(learningPrompt);
        _promptServiceMock.Setup(x => x.GetGuidedSystemPrompt()).Returns("guided");
        _promptServiceMock.Setup(x => x.GetExamSystemPrompt()).Returns("exam");

        TaskType captured = TaskType.Default;
        _llmServiceMock
            .Setup(x => x.GenerateNextMessageAsync(It.IsAny<List<Message>>(), It.IsAny<TaskType>(), It.IsAny<CancellationToken>()))
            .Callback<List<Message>, TaskType, CancellationToken>((_, tt, _) => captured = tt)
            .ReturnsAsync("bot");

        await _service.CreateMessageForUser(chat.Id, userId, "text", CancellationToken.None);

        captured.Should().Be(TaskType.Learning);
    }

    [Fact]
    public async Task CreateMessageForUser_ProblemSolverWithGuidedPrompt_PassesGuidedTaskTypeToLlm()
    {
        const string userId = "user1";
        const string guidedPrompt = "guided system prompt text";
        var chat = await SeedChatAsync(userId, ChatType.ProblemSolver);
        await SeedSystemMessageAsync(chat, guidedPrompt);

        _promptServiceMock.Setup(x => x.GetLearningSystemPrompt()).Returns("learning");
        _promptServiceMock.Setup(x => x.GetGuidedSystemPrompt()).Returns(guidedPrompt);
        _promptServiceMock.Setup(x => x.GetExamSystemPrompt()).Returns("exam");

        TaskType captured = TaskType.Default;
        _llmServiceMock
            .Setup(x => x.GenerateNextMessageAsync(It.IsAny<List<Message>>(), It.IsAny<TaskType>(), It.IsAny<CancellationToken>()))
            .Callback<List<Message>, TaskType, CancellationToken>((_, tt, _) => captured = tt)
            .ReturnsAsync("bot");

        await _service.CreateMessageForUser(chat.Id, userId, "text", CancellationToken.None);

        captured.Should().Be(TaskType.Guided);
    }

    [Fact]
    public async Task CreateMessageForUser_ProblemSolverWithExamPrompt_PassesExamTaskTypeToLlm()
    {
        const string userId = "user1";
        const string examPrompt = "exam system prompt text";
        var chat = await SeedChatAsync(userId, ChatType.ProblemSolver);
        await SeedSystemMessageAsync(chat, examPrompt);

        _promptServiceMock.Setup(x => x.GetLearningSystemPrompt()).Returns("learning");
        _promptServiceMock.Setup(x => x.GetGuidedSystemPrompt()).Returns("guided");
        _promptServiceMock.Setup(x => x.GetExamSystemPrompt()).Returns(examPrompt);

        TaskType captured = TaskType.Default;
        _llmServiceMock
            .Setup(x => x.GenerateNextMessageAsync(It.IsAny<List<Message>>(), It.IsAny<TaskType>(), It.IsAny<CancellationToken>()))
            .Callback<List<Message>, TaskType, CancellationToken>((_, tt, _) => captured = tt)
            .ReturnsAsync("bot");

        await _service.CreateMessageForUser(chat.Id, userId, "text", CancellationToken.None);

        captured.Should().Be(TaskType.Exam);
    }

    [Fact]
    public async Task CreateMessageForUser_ProblemSolverWithUnknownPrompt_PassesDefaultTaskTypeToLlm()
    {
        const string userId = "user1";
        var chat = await SeedChatAsync(userId, ChatType.ProblemSolver);
        await SeedSystemMessageAsync(chat, "some unknown prompt that matches nothing");

        _promptServiceMock.Setup(x => x.GetLearningSystemPrompt()).Returns("learning");
        _promptServiceMock.Setup(x => x.GetGuidedSystemPrompt()).Returns("guided");
        _promptServiceMock.Setup(x => x.GetExamSystemPrompt()).Returns("exam");

        TaskType captured = TaskType.Learning;
        _llmServiceMock
            .Setup(x => x.GenerateNextMessageAsync(It.IsAny<List<Message>>(), It.IsAny<TaskType>(), It.IsAny<CancellationToken>()))
            .Callback<List<Message>, TaskType, CancellationToken>((_, tt, _) => captured = tt)
            .ReturnsAsync("bot");

        await _service.CreateMessageForUser(chat.Id, userId, "text", CancellationToken.None);

        captured.Should().Be(TaskType.Default);
    }

    // BuildInitialMessages — tested via Create(chat, problemId, taskType)

    [Fact]
    public async Task CreateWithProblem_ExamTaskType_SavesThreeMessagesNoSolutionMsg()
    {
        const string userId = "user1";
        var problem = new Problem("llm solution", "problem statement", "Test Title") { TheoryLink = "link" };
        _context.Problems.Add(problem);
        await _context.SaveChangesAsync();

        SetupPromptMocksDefault();
        _llmServiceMock
            .Setup(x => x.GenerateNextMessageAsync(It.IsAny<List<Message>>(), It.IsAny<TaskType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("initial bot response");

        await _service.Create(new Chat { Name = "T", UserId = userId }, problem.Id, TaskType.Exam, CancellationToken.None);

        var messages = await _context.Messages.ToListAsync();
        // Exam: System + Condition + BotInitial = 3
        messages.Should().HaveCount(3);
        messages.Should().NotContain(m => m.MessageType == MessageType.User && m.IsSystemPrompt);
    }

    [Fact]
    public async Task CreateWithProblem_NonExamEmptySolution_SavesThreeMessagesNoSolutionMsg()
    {
        const string userId = "user1";
        var problem = new Problem("", "problem statement", "Test Title") { TheoryLink = "link" };
        _context.Problems.Add(problem);
        await _context.SaveChangesAsync();

        SetupPromptMocksDefault();
        _llmServiceMock
            .Setup(x => x.GenerateNextMessageAsync(It.IsAny<List<Message>>(), It.IsAny<TaskType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("initial bot response");

        await _service.Create(new Chat { Name = "T", UserId = userId }, problem.Id, TaskType.Learning, CancellationToken.None);

        var messages = await _context.Messages.ToListAsync();
        // Non-Exam + empty solution: System + Condition + BotInitial = 3
        messages.Should().HaveCount(3);
    }

    [Fact]
    public async Task CreateWithProblem_NonExamWithSolution_SavesFourMessagesIncludingSolutionMsg()
    {
        const string userId = "user1";
        var problem = new Problem("llm solution", "problem statement", "Test Title") { TheoryLink = "link" };
        _context.Problems.Add(problem);
        await _context.SaveChangesAsync();

        SetupPromptMocksDefault();
        _llmServiceMock
            .Setup(x => x.GenerateNextMessageAsync(It.IsAny<List<Message>>(), It.IsAny<TaskType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("initial bot response");

        await _service.Create(new Chat { Name = "T", UserId = userId }, problem.Id, TaskType.Learning, CancellationToken.None);

        var messages = await _context.Messages.ToListAsync();
        // Non-Exam + solution: System + Condition + Solution + BotInitial = 4
        messages.Should().HaveCount(4);
        messages.Should().Contain(m => m.MessageType == MessageType.User && m.IsSystemPrompt);
    }

    // AssociateUserTaskIfExistsAsync — tested via Create(chat, problemId, taskType)

    [Fact]
    public async Task CreateWithProblem_NoMatchingUserTask_LeavesUserTasksUntouched()
    {
        const string userId = "user1";
        var problem = new Problem("sol", "stmt", "title") { TheoryLink = "link" };
        _context.Problems.Add(problem);
        await _context.SaveChangesAsync();

        SetupPromptMocksDefault();
        _llmServiceMock.Setup(x => x.GenerateNextMessageAsync(It.IsAny<List<Message>>(), It.IsAny<TaskType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("bot");

        await _service.Create(new Chat { Name = "T", UserId = userId }, problem.Id, TaskType.Learning, CancellationToken.None);

        var userTasks = await _context.UserTasks.ToListAsync();
        userTasks.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateWithProblem_MatchingInProgressUserTask_SetsAssociatedChatId()
    {
        const string userId = "user1";
        var problem = new Problem("sol", "stmt", "title") { TheoryLink = "link" };
        _context.Problems.Add(problem);
        var problemTaskType = new ProblemTaskType(problem, TaskType.Learning);
        _context.ProblemTaskTypes.Add(problemTaskType);

        var userTask = new UserTask
        {
            ApplicationUserId = userId,
            ProblemId = problem.Id,
            ProblemHash = "hash",
            DisplayName = "Test",
            ProblemTaskType = problemTaskType,
            TaskType = TaskType.Learning,
            Status = UserTaskStatus.InProgress
        };
        _context.UserTasks.Add(userTask);
        await _context.SaveChangesAsync();

        SetupPromptMocksDefault();
        _llmServiceMock.Setup(x => x.GenerateNextMessageAsync(It.IsAny<List<Message>>(), It.IsAny<TaskType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("bot");

        var createdChat = await _service.Create(new Chat { Name = "T", UserId = userId }, problem.Id, TaskType.Learning, CancellationToken.None);

        var updatedTask = await _context.UserTasks.FindAsync(userTask.Id);
        updatedTask!.AssociatedChatId.Should().Be(createdChat.Id);
    }

    // CreateMessage empty LLM response

    [Fact]
    public async Task CreateMessageForUser_EmptyLlmResponse_DoesNotSaveBotMessage()
    {
        const string userId = "user1";
        var chat = await SeedChatAsync(userId, ChatType.Chat);
        await SeedSystemMessageAsync(chat, "sys");

        _llmServiceMock
            .Setup(x => x.GenerateNextMessageAsync(It.IsAny<List<Message>>(), It.IsAny<TaskType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(string.Empty);

        var before = await _context.Messages.CountAsync();
        await _service.CreateMessageForUser(chat.Id, userId, "hello", CancellationToken.None);
        var after = await _context.Messages.CountAsync();

        // Only the user message added; empty LLM response means no bot message
        (after - before).Should().Be(1);
    }

    // GetChatDetailsInternalAsync — tested via public methods

    [Fact]
    public async Task GetChatDetailsAsync_NonProblemSolverChat_ReturnsNullTaskTypeAndLink()
    {
        const string userId = "user1";
        var chat = await SeedChatAsync(userId, ChatType.Chat);

        var result = await _service.GetChatDetailsAsync(chat.Id, userId, CancellationToken.None);

        result.TaskType.Should().BeNull();
        result.TheoryLink.Should().BeNull();
    }

    [Fact]
    public async Task GetChatDetailsAsync_ProblemSolverChatNoUserTask_ReturnsNullTaskTypeAndLink()
    {
        const string userId = "user1";
        var chat = await SeedChatAsync(userId, ChatType.ProblemSolver);

        var result = await _service.GetChatDetailsAsync(chat.Id, userId, CancellationToken.None);

        result.TaskType.Should().BeNull();
        result.TheoryLink.Should().BeNull();
    }
}
