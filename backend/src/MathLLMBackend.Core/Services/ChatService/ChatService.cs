using System.Text;
using MathLLMBackend.Core.Configuration;
using MathLLMBackend.Core.Services.LlmService;
using MathLLMBackend.Core.Services.PromptService;
using MathLLMBackend.DataAccess.Contexts;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Domain.Enums;
using MathLLMBackend.GeolinClient;
using MathLLMBackend.GeolinClient.Models;
using MathLLMBackend.Core.Services.ProblemsService;
using MathLLMBackend.ProblemsClient.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices; // Added for EnumeratorCancellation

namespace MathLLMBackend.Core.Services.ChatService;

public class ChatService : IChatService
{
    private readonly AppDbContext _dbContext;
    private readonly ILlmService _llmService;
    private readonly IProblemsService _problemsService;
    private readonly IPromptService _promptService;
    private readonly IOptions<LlmServiceConfiguration> _llmConfig;
    private readonly ILogger<ChatService> _logger;

    public ChatService(
        AppDbContext dbContext, 
        ILlmService llmService, 
        IProblemsService problemsService,
        IPromptService promptService,
        IOptions<LlmServiceConfiguration> llmConfig,
        ILogger<ChatService> logger)
    {
        _dbContext = dbContext;
        _llmService = llmService;
        _problemsService = problemsService;
        _promptService = promptService;
        _llmConfig = llmConfig;
        _logger = logger;
    }
    
    public async Task<Chat> Create(Chat chat, CancellationToken ct)
    {
        chat.Type = ChatType.Chat;
        var res = await _dbContext.Chats.AddAsync(chat, ct);
        await _dbContext.Messages.AddAsync(
            new Message(
                res.Entity,
                _promptService.GetDefaultSystemPrompt(),
                MessageType.System)
            , ct);
        
        await _dbContext.SaveChangesAsync(ct);
        return res.Entity;
    }

    public async Task<Chat> Create(Chat chat, string problemDbId, int explicitTaskType, CancellationToken ct)
    {
        chat.Type = ChatType.ProblemSolver;
        _logger.LogInformation("Creating chat for ProblemSolver. ProblemDB_ID: {ProblemDbId}, ExplicitTaskType: {ExplicitTaskType}", problemDbId, explicitTaskType);

        // 1. Получаем задачу из нашей базы LLMath-Problems по ID
        var problemFromDb = await _problemsService.GetProblemFromDbAsync(problemDbId, ct);

        if (problemFromDb == null)
        {
            _logger.LogError("Problem with ID {ProblemDbId} not found in LLMath-Problems database.", problemDbId);
            // Можно выбросить исключение или вернуть null/ошибку, чтобы это обработалось выше
            // Пока что просто логируем и создаем чат без условия/решения, что не очень хорошо
            // Лучше выбросить исключение, чтобы StartUserTask его поймал.
            throw new KeyNotFoundException($"Problem with ID {problemDbId} not found in LLMath-Problems database.");
        }

        string problemCondition = problemFromDb.Statement;
        // Берём заранее сгенерированное решение из БД
        string? llmSolution = problemFromDb.LlmSolution is string sol && !string.IsNullOrWhiteSpace(sol)
            ? sol
            : problemFromDb.LlmSolution?.ToString();
        if (string.IsNullOrWhiteSpace(llmSolution))
        {
            _logger.LogWarning("Problem {ProblemDbId} from DB has no LLM solution. Tutor solution will not be included.", problemDbId);
        }
        
        _logger.LogInformation("Using problem: {ProblemTitle}, Condition snippet: {ConditionSnippet}", 
            problemFromDb.Id, // Используем ID вместо Title
            problemCondition.Substring(0, Math.Min(50, problemCondition.Length)) + "...");

        var addedChatEntityEntry = await _dbContext.Chats.AddAsync(chat, ct);
        await _dbContext.SaveChangesAsync(ct); 
        var newChatEntity = addedChatEntityEntry.Entity;

        int taskTypeToUse = explicitTaskType;
        string systemPromptText = _promptService.GetSystemPromptByTaskType(taskTypeToUse);
        
        var userTask = await _dbContext.UserTasks
            .FirstOrDefaultAsync(ut => ut.ProblemHash == problemDbId && 
                                   ut.ApplicationUserId == chat.UserId && 
                                   ut.Status == UserTaskStatus.InProgress, ct);
        if (userTask != null)
        {
            userTask.AssociatedChatId = newChatEntity.Id;
            _dbContext.UserTasks.Update(userTask);
        }
        
        var systemMessage = new Message(newChatEntity, systemPromptText, MessageType.System);
        Message? solutionMessageForLlm = null;
        if (taskTypeToUse != 3 && !string.IsNullOrWhiteSpace(llmSolution)) // Не для режима Экзамена и если решение есть
        {
            var tutorSolutionText = _promptService.GetTutorSolutionPrompt(llmSolution); // Передаем готовое решение LLM
            solutionMessageForLlm = new Message(newChatEntity, tutorSolutionText, MessageType.User, isSystemPrompt: true);
        }
        
        var conditionTextForDisplay = $"🟢 **Условие задачи:** ({problemFromDb.Id})\n\n{problemCondition}\n\n";
        var conditionMessageForDisplay = new Message(newChatEntity, conditionTextForDisplay, MessageType.Assistant);
        
        var messagesToSaveInDb = new List<Message> { systemMessage, conditionMessageForDisplay };
        if (solutionMessageForLlm != null)
        {
             messagesToSaveInDb.Add(solutionMessageForLlm);
        }
        await _dbContext.Messages.AddRangeAsync(messagesToSaveInDb, ct);
        
        await _dbContext.SaveChangesAsync(ct);
        
        var initialPromptForLlm = _promptService.GetInitialPromptByTaskType(taskTypeToUse, problemCondition, "");
        var messagesForInitialBotGeneration = new List<Message> { systemMessage };
        if (solutionMessageForLlm != null) messagesForInitialBotGeneration.Add(solutionMessageForLlm);
        // Передаем оригинальное условие задачи LLM для генерации первого сообщения, а не форматированное
        messagesForInitialBotGeneration.Add(new Message(newChatEntity, problemCondition, MessageType.User, isSystemPrompt: true)); 
        messagesForInitialBotGeneration.Add(new Message(newChatEntity, initialPromptForLlm, MessageType.User, isSystemPrompt: true));

        _logger.LogInformation("Start initial LLM generation for chat {ChatId} | taskType = {TaskTypeToUse}", newChatEntity.Id, taskTypeToUse);
        var initialBotMessageText = await _llmService.GenerateNextMessageAsync(messagesForInitialBotGeneration, taskTypeToUse, ct);
        _logger.LogInformation("Initial bot message generated for chat {ChatId} | taskType = {TaskTypeToUse}", newChatEntity.Id, taskTypeToUse);
        
        var botInitialDisplayMessage = new Message(newChatEntity, initialBotMessageText, MessageType.Assistant);
        await _dbContext.Messages.AddAsync(botInitialDisplayMessage, ct);
        await _dbContext.SaveChangesAsync(ct);
        
        return newChatEntity;
    }

    public async Task<List<Chat>> GetUserChats(string userId, CancellationToken ct)
    {
        var chats = await _dbContext.Chats.Where(c => c.User.Id == userId).ToListAsync(cancellationToken: ct);
        // Не уверен, нужен ли здесь SaveChangesAsync, так как это операция чтения
        // await _dbContext.SaveChangesAsync(ct);
        return chats;
    }

    public async Task Delete(Chat chat, CancellationToken ct)
    {
        _dbContext.Chats.Remove(chat);
        await _dbContext.SaveChangesAsync(ct);
    }
    
    public async IAsyncEnumerable<string> CreateMessage(Message message, [EnumeratorCancellation] CancellationToken ct)
    {
        await _dbContext.Messages.AddAsync(message, ct); // Сохраняем сообщение пользователя
        await _dbContext.SaveChangesAsync(ct); // Сохраняем сразу, чтобы оно было в истории для LLM

        var currentChat = await _dbContext.Chats
            .Include(c => c.Messages) // Загружаем все сообщения чата
            .FirstOrDefaultAsync(c => c.Id == message.ChatId, ct);

        if (currentChat == null)
        {
            _logger.LogError("Chat with ID {ChatId} not found in CreateMessage.", message.ChatId);
            yield break; 
        }

        // Определяем taskType для текущего чата.
        // Это упрощенный вариант. В идеале, taskType должен храниться в самом Chat или UserTask.
        int taskType = 0; 
        var systemMessageInHistory = currentChat.Messages.FirstOrDefault(m => m.MessageType == MessageType.System);
        
        if (currentChat.Type == ChatType.ProblemSolver)
        {
             var userTask = await _dbContext.UserTasks
                .FirstOrDefaultAsync(ut => ut.AssociatedChatId == currentChat.Id, ct);
            if (userTask != null) taskType = userTask.TaskType;
            else if (systemMessageInHistory != null) // Пытаемся угадать по системному промпту если UserTask нет
            {
                if(systemMessageInHistory.Text == _promptService.GetLearningSystemPrompt()) taskType = 1;
                else if(systemMessageInHistory.Text == _promptService.GetGuidedSystemPrompt()) taskType = 2;
                else if(systemMessageInHistory.Text == _promptService.GetExamSystemPrompt()) taskType = 3;
            }
        }
        // Для ChatType.Chat taskType останется 0 (Default/Tutor)
        
        _logger.LogInformation("Generating response in chat {ChatId} | taskType = {TaskType}", currentChat.Id, taskType);
        
        var messagesForLlm = currentChat.Messages.ToList();
        
        // В режиме экзамена (3) не передаем LLM скрытое решение, если оно там было
        if (taskType == 3)
        {
            messagesForLlm.RemoveAll(m => m.IsSystemPrompt && m.MessageType == MessageType.User && m.Text.Contains("Вот правильное решение задачи для вашего руководства"));
        }

        var llmResponseStream = _llmService.GenerateNextMessageStreaming(messagesForLlm, taskType, ct);

        var fullText = new StringBuilder();
        await foreach (var messageTextChunk in llmResponseStream)
        {
            fullText.Append(messageTextChunk);
            yield return messageTextChunk;
        }
        
        var newAssistantMessage = new Message(currentChat, fullText.ToString(), MessageType.Assistant);
        _dbContext.Messages.Add(newAssistantMessage);
        await _dbContext.SaveChangesAsync(ct);
    }
    
    public async Task<List<Message>> GetAllMessageFromChat(Chat chat, CancellationToken ct)
    {
        return await _dbContext.Messages.Where(m => m.ChatId == chat.Id).OrderBy(m=>m.CreatedAt).ToListAsync(ct);
    }

    public async Task<Chat?> GetChatById(Guid id, CancellationToken ct)
    {
        return await _dbContext.Chats
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken: ct);
    }

    public async Task<Guid> GetOrCreateProblemChatAsync(string problemHash, string userId, string taskDisplayName, int taskType, CancellationToken ct)
    {
        var chatName = $"{taskDisplayName} {DateTime.Now:dd.MM.yyyy HH:mm}";
        var newChatEntity = new Chat
        {
            Name = chatName,
            UserId = userId,
            Type = ChatType.ProblemSolver // Явно указываем тип
        };

        // Передаем taskType в метод Create
        var createdChat = await Create(newChatEntity, problemHash, taskType, ct);
        return createdChat.Id;
    }

    public async Task<Message?> GetMessageId(Guid id, CancellationToken ct)
    {
        return await _dbContext.Messages.FirstOrDefaultAsync(c => c.Id == id, cancellationToken: ct);
    }
}
