using MathLLMBackend.Core.Constants;
using MathLLMBackend.Core.Services.LlmService;
using MathLLMBackend.Core.Services.PromptService;
using MathLLMBackend.Core.Contexts;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Domain.Enums;
using MathLLMBackend.Domain.Exceptions;
using MathLLMBackend.Core.Services.ProblemsService;
using MathLLMBackend.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MathLLMBackend.Core.Services.ChatService;

public class ChatService(
    IAppDbContext dbContext,
    ILlmService llmService,
    IPromptService promptService,
    ILogger<ChatService> logger)
    : IChatService
{
    private readonly IAppDbContext _dbContext = dbContext;
    private readonly ILlmService _llmService = llmService;
    private readonly IPromptService _promptService = promptService;
    private readonly ILogger<ChatService> _logger = logger;

    public async Task<Chat> Create(Chat chat, CancellationToken ct)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

        chat.Type = ChatType.Chat;
        var chatEntry = await _dbContext.Chats.AddAsync(chat, ct);

        var systemMessage = new Message(
            chatEntry.Entity,
            _promptService.GetDefaultSystemPrompt(),
            MessageType.System);

        await _dbContext.Messages.AddAsync(systemMessage, ct);
        await _dbContext.SaveChangesAsync(ct);

        await transaction.CommitAsync(ct);

        return chatEntry.Entity;
    }

    public async Task<Chat> Create(Chat chat, Guid problemId, TaskType explicitTaskType, CancellationToken ct)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);

        chat.Type = ChatType.ProblemSolver;
        _logger.LogInformation("Creating chat for ProblemSolver. ProblemDB_ID: {ProblemDbId}, ExplicitTaskType: {ExplicitTaskType}",
            problemId, explicitTaskType);

        var problem = await GetProblem(problemId, ct);
        var llmSolution = ExtractLlmSolution(problem);

        var newChat = await CreateChatEntityAsync(chat, ct);
        await AssociateUserTaskIfExistsAsync(newChat, problemId, chat.UserId, ct);

        var messages = BuildInitialMessages(newChat, problem, llmSolution, explicitTaskType);
        await _dbContext.Messages.AddRangeAsync(messages, ct);
        await _dbContext.SaveChangesAsync(ct);

        var initialBotMessage = await GenerateInitialBotMessageAsync(newChat, problem, llmSolution, explicitTaskType, ct);
        await _dbContext.Messages.AddAsync(initialBotMessage, ct);
        await _dbContext.SaveChangesAsync(ct);

        await transaction.CommitAsync(ct);

        return newChat;
    }

    private async Task<Problem> GetProblem(Guid id, CancellationToken ct)
    {
        var problem = await _dbContext.Problems
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (problem == null)
        {
            _logger.LogError("Problem with ID {ProblemDbId} not found in LLMath-Problems database.", id);
            throw new NotFoundException($"Problem with ID {id} not found in LLMath-Problems database.");
        }

        var conditionSnippet = problem.Statement.Length > DisplayConstants.MaxSnippetLength
            ? problem.Statement[..DisplayConstants.MaxSnippetLength] + "..."
            : problem.Statement;

        _logger.LogInformation("Using problem: {ProblemId}, Condition snippet: {ConditionSnippet}",
            problem.Id, conditionSnippet);

        return problem;
    }

    private static string? ExtractLlmSolution(Problem problem)
    {
        return problem.LlmSolution is string sol && !string.IsNullOrWhiteSpace(sol)
            ? sol
            : problem.LlmSolution?.ToString();
    }

    private async Task<Chat> CreateChatEntityAsync(Chat chat, CancellationToken ct)
    {
        var chatEntry = await _dbContext.Chats.AddAsync(chat, ct);
        await _dbContext.SaveChangesAsync(ct);
        return chatEntry.Entity;
    }

    private async Task AssociateUserTaskIfExistsAsync(Chat chat, Guid problemDbId, string userId, CancellationToken ct)
    {
        var userTask = await _dbContext.UserTasks
            .FirstOrDefaultAsync(ut => ut.ProblemId == problemDbId
                && ut.ApplicationUserId == userId
                && ut.Status == UserTaskStatus.InProgress, ct);

        if (userTask != null)
        {
            userTask.AssociatedChatId = chat.Id;
            _dbContext.UserTasks.Update(userTask);
            await _dbContext.SaveChangesAsync(ct);
        }
    }

    private List<Message> BuildInitialMessages(Chat chat, Problem problem, string? llmSolution, TaskType taskType)
    {
        var systemPrompt = _promptService.GetSystemPromptByTaskType(taskType);
        var systemMessage = new Message(chat, systemPrompt, MessageType.System);

        var messages = new List<Message> { systemMessage };

        var formattedCondition = FormatProblemConditionForDisplay(problem);
        var conditionMessage = new Message(chat, formattedCondition, MessageType.Assistant);
        messages.Add(conditionMessage);

        if (taskType != TaskType.Exam && !string.IsNullOrWhiteSpace(llmSolution))
        {
            var tutorSolutionPrompt = _promptService.GetTutorSolutionPrompt(llmSolution);
            var solutionMessage = new Message(chat, tutorSolutionPrompt, MessageType.User, isSystemPrompt: true);
            messages.Add(solutionMessage);
        }

        return messages;
    }

    private static string FormatProblemConditionForDisplay(Problem problem)
    {
        var fixedCondition = problem.Statement.Replace("\r\n", "\\\\").Replace("\n", "\\\\");
        return $"**Условие задачи:** ({problem.Id})<br/><br/>\n\n{fixedCondition}\n\n";
    }

    private async Task<Message> GenerateInitialBotMessageAsync(
        Chat chat,
        Problem problem,
        string? llmSolution,
        TaskType taskType,
        CancellationToken ct)
    {
        var systemPrompt = _promptService.GetSystemPromptByTaskType(taskType);
        var systemMessage = new Message(chat, systemPrompt, MessageType.System);

        var messagesForLlm = new List<Message> { systemMessage };

        if (taskType != TaskType.Exam && !string.IsNullOrWhiteSpace(llmSolution))
        {
            var tutorSolutionPrompt = _promptService.GetTutorSolutionPrompt(llmSolution);
            messagesForLlm.Add(new Message(chat, tutorSolutionPrompt, MessageType.User, isSystemPrompt: true));
        }

        messagesForLlm.Add(new Message(chat, problem.Statement, MessageType.User, isSystemPrompt: true));

        var initialPrompt = _promptService.GetInitialPromptByTaskType(taskType, problem.Statement, string.Empty);
        messagesForLlm.Add(new Message(chat, initialPrompt, MessageType.User, isSystemPrompt: true));

        _logger.LogInformation("Starting initial LLM generation for chat {ChatId} | taskType = {TaskType}",
            chat.Id, taskType);

        var botMessageText = await _llmService.GenerateNextMessageAsync(messagesForLlm, taskType, ct);

        _logger.LogInformation("Initial bot message generated for chat {ChatId} | taskType = {TaskType}",
            chat.Id, taskType);

        return new Message(chat, botMessageText, MessageType.Assistant);
    }

    public async Task<List<Chat>> GetUserChats(string userId, CancellationToken ct)
    {
        var chats = await _dbContext.Chats.Where(c => c.User.Id == userId).ToListAsync(cancellationToken: ct);
        return chats;
    }

    public async Task Delete(Guid chatId, string userId, CancellationToken ct)
    {
        var chat = await GetChatByIdForUser(chatId, userId, ct);
        _dbContext.Chats.Remove(chat);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<string> CreateMessageForUser(Guid chatId, string userId, string text, CancellationToken ct)
    {
        var chat = await GetChatByIdForUser(chatId, userId, ct);
        var message = new Message(chat, text, MessageType.User);
        return await CreateMessage(message, ct);
    }

    private async Task<string> CreateMessage(Message message, CancellationToken ct)
    {
        var chatId = message.Chat?.Id ?? message.ChatId;

        var currentChat = await _dbContext.Chats
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == chatId, ct);

        if (currentChat == null)
        {
            _logger.LogError("Chat with ID {ChatId} not found in CreateMessage.", chatId);
            throw new NotFoundException($"Chat with ID {chatId} not found.");
        }

        var taskType = await DetermineTaskTypeAsync(currentChat, ct);

        _logger.LogInformation("Generating (full) response in chat {ChatId} | taskType = {TaskType}", currentChat.Id, taskType);

        var messagesForLlm = currentChat.Messages.ToList();
        messagesForLlm.Add(message);

        if (taskType == TaskType.Exam)
        {
            messagesForLlm.RemoveAll(m => m.IsSystemPrompt && m.Text.Contains(MessageConstants.TutorSolutionMarker));
        }

        var llmResponseText = await _llmService.GenerateNextMessageAsync(messagesForLlm, taskType, ct);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
        await _dbContext.Messages.AddAsync(message, ct);

        if (!string.IsNullOrEmpty(llmResponseText))
        {
            var botMessage = new Message(currentChat, llmResponseText, MessageType.Assistant);
            await _dbContext.Messages.AddAsync(botMessage, ct);
            _logger.LogInformation("LLM full response saved for chat {ChatId}. Length: {Length}", currentChat.Id, llmResponseText.Length);
        }
        else
        {
            _logger.LogWarning("LLM returned empty or null full response for chat {ChatId}", currentChat.Id);
        }

        await _dbContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return llmResponseText;
    }

    private async Task<TaskType> DetermineTaskTypeAsync(Chat currentChat, CancellationToken ct)
    {
        var taskType = TaskType.Default;

        if (currentChat.Type != ChatType.ProblemSolver)
            return taskType;

        var userTask = await _dbContext.UserTasks.Include(userTask => userTask.ProblemTaskType)
            .FirstOrDefaultAsync(ut => ut.AssociatedChatId == currentChat.Id, ct);
        taskType = userTask?.ProblemTaskType.TaskType ?? DetermineTaskTypeFromSystemPrompt(currentChat);

        return taskType;
    }

    private TaskType DetermineTaskTypeFromSystemPrompt(Chat chat)
    {
        var systemMessage = chat.Messages.FirstOrDefault(m => m.MessageType == MessageType.System);

        if (systemMessage == null)
        {
            _logger.LogWarning("No system prompt found to determine taskType for chat {ChatId}", chat.Id);
            return TaskType.Default;
        }

        var systemPromptText = systemMessage.Text;

        if (systemPromptText == _promptService.GetLearningSystemPrompt())
            return TaskType.Learning;

        if (systemPromptText == _promptService.GetGuidedSystemPrompt())
            return TaskType.Guided;

        if (systemPromptText == _promptService.GetExamSystemPrompt())
            return TaskType.Exam;

        _logger.LogWarning("Could not determine taskType from system prompt for chat {ChatId}", chat.Id);
        return TaskType.Default;
    }

    private async Task<List<Message>> GetAllMessageFromChat(Chat chat, CancellationToken ct)
    {
        return await _dbContext.Messages
            .Where(m => m.ChatId == chat.Id)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(ct);
    }

    private async Task<List<Message>> GetAllMessagesFromChatForUser(Guid chatId, string userId, CancellationToken ct)
    {
        var chat = await GetChatByIdForUser(chatId, userId, ct);
        return await GetAllMessageFromChat(chat, ct);
    }

    public async Task<List<Message>> GetUserVisibleMessagesFromChat(Guid chatId, string userId, CancellationToken ct)
    {
        var allMessages = await GetAllMessagesFromChatForUser(chatId, userId, ct);
        return allMessages.Where(m => !m.IsSystemPrompt).ToList();
    }

    private async Task<Chat?> GetChatById(Guid id, CancellationToken ct)
    {
        var chat = await _dbContext.Chats
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken: ct);

        if (chat != null)
        {
            await _dbContext.Entry(chat)
                .Reference(c => c.User)
                .LoadAsync(ct);
        }

        return chat;
    }

    public async Task<Chat?> GetChatByIdAsync(Guid chatId, CancellationToken ct)
    {
        return await GetChatById(chatId, ct);
    }

    public async Task<List<Message>> GetUserVisibleMessagesFromChatForAdmin(Guid chatId, CancellationToken ct)
    {
        var chat = await GetChatById(chatId, ct);
        if (chat == null)
        {
            throw new NotFoundException($"Chat with ID {chatId} not found.");
        }

        var allMessages = await GetAllMessageFromChat(chat, ct);
        return allMessages.Where(m => !m.IsSystemPrompt).ToList();
    }

    public async Task<Chat> GetChatByIdForUser(Guid chatId, string userId, CancellationToken ct)
    {
        var chat = await GetChatById(chatId, ct);

        if (chat == null)
        {
            throw new NotFoundException($"Chat with ID {chatId} not found.");
        }

        if (chat.UserId != userId)
        {
            throw new AuthorizationException("You do not have permission to access this chat.");
        }

        return chat;
    }

    public async Task<Guid> GetOrCreateProblemChatAsync(Guid problemId, string userId, string taskDisplayName, TaskType taskType, CancellationToken ct)
    {
        var chatName = $"{taskDisplayName} {DateTime.Now:dd.MM.yyyy HH:mm}";
        var newChat = new Chat
        {
            Name = chatName,
            UserId = userId,
            Type = ChatType.ProblemSolver
        };

        var createdChat = await Create(newChat, problemId, taskType, ct);
        return createdChat.Id;
    }

    public async Task<ChatDetailsModel> GetChatDetailsAsync(Guid chatId, string userId, CancellationToken ct)
    {
        await GetChatByIdForUser(chatId, userId, ct);
        return await GetChatDetailsInternalAsync(chatId, ct);
    }

    public async Task<ChatDetailsModel> GetChatDetailsForAdminAsync(Guid chatId, CancellationToken ct)
    {
        var chat = await GetChatById(chatId, ct);
        if (chat == null)
        {
            throw new NotFoundException($"Chat with ID {chatId} not found.");
        }
        return await GetChatDetailsInternalAsync(chatId, ct);
    }

    private async Task<ChatDetailsModel> GetChatDetailsInternalAsync(Guid chatId, CancellationToken ct)
    {
        var chat = await GetChatById(chatId, ct);

        if (chat!.Type != ChatType.ProblemSolver)
        {
            return new ChatDetailsModel(null, null);
        }

        var userTask = await _dbContext.UserTasks
            .AsNoTracking().Include(userTask => userTask.ProblemTaskType)
            .FirstOrDefaultAsync(ut => ut.AssociatedChatId == chatId, ct);

        if (userTask == null)
        {
            return new ChatDetailsModel(null, null);
        }

        TaskType? taskType = userTask.ProblemTaskType.TaskType;
        var problem = await _dbContext.Problems.FirstOrDefaultAsync(p => p.Id == userTask.ProblemId, ct);
        var theoryLink = problem?.TheoryLink;

        return new ChatDetailsModel(taskType, theoryLink);
    }
}
