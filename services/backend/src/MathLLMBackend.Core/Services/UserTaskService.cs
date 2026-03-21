using MathLLMBackend.Core.Constants;
using MathLLMBackend.Core.Services.ChatService;
using MathLLMBackend.DataAccess.Contexts;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MathLLMBackend.Core.Services.ProblemsService;
using MathLLMBackend.Domain.Exceptions;

namespace MathLLMBackend.Core.Services;

public class UserTaskService(
    AppDbContext context,
    IProblemsService problemsService,
    IChatService chatService,
    ILogger<UserTaskService> logger)
    : IUserTaskService
{
    private readonly AppDbContext _context = context;
    private readonly IProblemsService _problemsService = problemsService;
    private readonly IChatService _chatService = chatService;
    private readonly ILogger<UserTaskService> _logger = logger;

    private static readonly Dictionary<TaskType, string> TaskTypeToProblemTypeName = new()
    {
        { TaskType.Learning, "problems" },
        { TaskType.Guided, "problems" },
        { TaskType.Exam, "problems" }
    };

    public async Task<IEnumerable<UserTask>> GetOrCreateUserTasksAsync(string userId, TaskType taskType, CancellationToken cancellationToken = default)
    {
        IEnumerable<Problem> problems;
        try
        {
            problems = (await _problemsService.GetProblemsByType(taskType, cancellationToken)).ToArray();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while getting problems by type {TaskType}", taskType);
            return [];
        }

        if (!problems.Any())
        {
            _logger.LogInformation("No problems of type '{TypeName}' found", taskType);
            return [];
        }

        var newOrExistingUserTasks = new List<UserTask>();

        foreach (var problem in problems)
        {
            var existingUserTask = await _context.UserTasks
                .FirstOrDefaultAsync(ut => ut.ApplicationUserId == userId && ut.ProblemId == problem.Id, cancellationToken: cancellationToken);
            
            if (existingUserTask != null)
            {
                newOrExistingUserTasks.Add(existingUserTask);
            }
            else
            {
                var displayName = !string.IsNullOrWhiteSpace(problem.Title)
                    ? problem.Title
                    : GetTruncatedStatement(problem.Statement);
                
                var newTask = new UserTask
                {
                    ApplicationUserId = userId,
                    ProblemId = problem.Id,
                    DisplayName = displayName,
                    TaskType = taskType,
                    ProblemTaskType = problem.Types.First(t => t.TaskType == taskType),
                    Status = UserTaskStatus.NotStarted,
                    AssociatedChatId = null,
                    ProblemHash = problem.Id.ToString()
                };
                
                _context.UserTasks.Add(newTask);
                newOrExistingUserTasks.Add(newTask);
            }
        }
        
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Returning {Count} UserTasks based on LLMath-Problems DB for user {UserId}", newOrExistingUserTasks.Count, userId);
        return newOrExistingUserTasks.OrderBy(ut => ut.DisplayName);
    }

    public async Task<UserTask> StartUserTaskWithChatAsync(Guid userTaskId, string userId, CancellationToken cancellationToken = default)
    {
        var userTask = await GetUserTaskByIdAsync(userTaskId, userId, cancellationToken);
        if (userTask == null)
        {
            throw new NotFoundException($"Task not found or you don't have permission.");
        }

        if (userTask.AssociatedChatId.HasValue && userTask.AssociatedChatId != Guid.Empty)
        {
            _logger.LogInformation("Task {UserTaskId} already associated with chat {ChatId}. Returning current state.", userTaskId, userTask.AssociatedChatId);
            var confirmedTask = await StartTaskAsync(userTaskId, userTask.AssociatedChatId.Value, userId, cancellationToken);
            return confirmedTask ?? throw new InvalidOperationException("Failed to confirm task state.");
        }

        var chatId = await _chatService.GetOrCreateProblemChatAsync(
            userTask.ProblemId, 
            userId, 
            userTask.DisplayName, 
            userTask.ProblemTaskType.TaskType,
            cancellationToken);

        if (chatId == Guid.Empty)
        {
            throw new InvalidOperationException("Failed to obtain a valid chat ID.");
        }

        var updatedTask = await StartTaskAsync(userTaskId, chatId, userId, cancellationToken);
        if (updatedTask == null)
        {
            throw new InvalidOperationException("Failed to update task status.");
        }

        return updatedTask;
    }

    public async Task<UserTask?> StartTaskAsync(Guid userTaskId, Guid chatId, string userId, CancellationToken cancellationToken = default)
    {
        var userTask = await _context.UserTasks
            .FirstOrDefaultAsync(ut => ut.Id == userTaskId && ut.ApplicationUserId == userId, cancellationToken);

        if (userTask == null)
        {
            _logger.LogWarning("UserTask with ID {UserTaskId} not found for user {UserId}", userTaskId, userId);
            return null;
        }

        if (userTask.Status == UserTaskStatus.InProgress && userTask.AssociatedChatId == chatId)
        {
            _logger.LogInformation("Task {UserTaskId} is already in progress with chat {ChatId}.", userTaskId, chatId);
            return userTask;
        }
        
        if (userTask.AssociatedChatId != null && userTask.AssociatedChatId != chatId)
        {
             _logger.LogWarning("Task {UserTaskId} is already associated with a different chat {ExistingChatId}. Cannot associate with new chat {NewChatId}.", 
                userTaskId, userTask.AssociatedChatId, chatId);
            return null; 
        }

        userTask.Status = UserTaskStatus.InProgress;
        userTask.AssociatedChatId = chatId;

        _context.UserTasks.Update(userTask);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Task {UserTaskId} status updated to InProgress and associated with chat {ChatId}", userTaskId, chatId);

        return userTask;
    }

    public async Task<UserTask?> GetUserTaskByIdAsync(Guid userTaskId, string userId, CancellationToken cancellationToken = default)
    {
        var userTask = await _context.UserTasks
            .Include(ut => ut.ProblemTaskType)
            .FirstOrDefaultAsync(ut => ut.Id == userTaskId && ut.ApplicationUserId == userId, cancellationToken);
        
        if (userTask == null)
        {
            _logger.LogWarning("UserTask with ID {UserTaskId} not found for user {UserId} in GetUserTaskByIdAsync.", userTaskId, userId);
            return null;
        }
        return userTask;
    }

    public async Task<UserTask?> CompleteTaskAsync(Guid userTaskId, string userId, CancellationToken cancellationToken = default)
    {
        var userTask = await _context.UserTasks
            .Include(ut => ut.ProblemTaskType)
            .FirstOrDefaultAsync(ut => ut.Id == userTaskId && ut.ApplicationUserId == userId, cancellationToken);

        if (userTask == null)
        {
            _logger.LogWarning("CompleteTask: UserTask with ID {UserTaskId} not found for user {UserId}", userTaskId, userId);
            return null;
        }

        if (userTask.Status == UserTaskStatus.Solved)
        {
            _logger.LogInformation("CompleteTask: Task {UserTaskId} is already marked as solved.", userTaskId);
            return userTask;
        }

        userTask.Status = UserTaskStatus.Solved;
        _context.UserTasks.Update(userTask);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("CompleteTask: Task {UserTaskId} marked as solved for user {UserId}", userTaskId, userId);

        return userTask;
    }

    private static string GetTruncatedStatement(string statement)
    {
        return statement.Length > DisplayConstants.MaxSnippetLength 
            ? statement.Substring(0, DisplayConstants.MaxSnippetLength) + "..." 
            : statement;
    }

} 