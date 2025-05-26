using System.Security.Claims;
using MathLLMBackend.Core.Services;
using MathLLMBackend.Core.Dtos;
using MathLLMBackend.Core.Services.ChatService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MathLLMBackend.Presentation.Controllers;

[Authorize] // Все методы требуют авторизации
[ApiController]
[Route("api/[controller]")]
public class UserTasksController : ControllerBase
{
    private readonly IUserTaskService _userTaskService;
    private readonly IChatService _chatService;
    private readonly ILogger<UserTasksController> _logger;

    public UserTasksController(
        IUserTaskService userTaskService, 
        IChatService chatService,
        ILogger<UserTasksController> logger)
    {
        _userTaskService = userTaskService;
        _chatService = chatService;
        _logger = logger;
    }

    /// <summary>
    /// Получает или создает задачи пользователя для указанного типа.
    /// </summary>
    /// <param name="taskType">Тип задач (например, 0 для задач из списка "Выбрать задачу").</param>
    /// <returns>Список задач пользователя.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserTaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserTasks([FromQuery] int taskType = 0) // По умолчанию тип 0
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User ID not found in token.");
            return Unauthorized("User ID not found in token.");
        }

        try
        {
            var tasks = await _userTaskService.GetOrCreateUserTasksAsync(userId, taskType, HttpContext.RequestAborted);
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting or creating user tasks for user {UserId} and type {TaskType}", userId, taskType);
            // В зависимости от типа ошибки можно возвращать разные статусы
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
        }
    }

    /// <summary>
    /// Помечает задачу как начатую (InProgress) и создает/связывает ее с чатом.
    /// </summary>
    /// <param name="userTaskId">ID задачи пользователя.</param>
    /// <returns>Обновленная задача пользователя.</returns>
    [HttpPost("{userTaskId:guid}/start")]
    [ProducesResponseType(typeof(UserTaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> StartUserTask(Guid userTaskId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User ID not found in token for starting task.");
            return Unauthorized("User ID not found in token.");
        }

        try
        {
            // 1. Получаем UserTask, чтобы взять ProblemHash и DisplayName
            var userTask = await _userTaskService.GetUserTaskByIdAsync(userTaskId, userId, HttpContext.RequestAborted);
            if (userTask == null)
            {
                _logger.LogWarning("StartTask: UserTask with ID {UserTaskId} not found for user {UserId}", userTaskId, userId);
                return NotFound("Task not found or you don't have permission.");
            }
            
            // Проверка, если чат уже связан (на всякий случай, хотя фронтенд не должен вызывать start для таких)
            if (userTask.AssociatedChatId.HasValue && userTask.AssociatedChatId != Guid.Empty)
            {
                 _logger.LogInformation("StartTask: Task {UserTaskId} already associated with chat {ChatId}. Returning current state.", userTaskId, userTask.AssociatedChatId);
                 var currentDto = await _userTaskService.StartTaskAsync(userTaskId, userTask.AssociatedChatId.Value, userId, HttpContext.RequestAborted);
                 if (currentDto == null)
                 {
                    // Этого не должно происходить, если задача действительно уже была правильно начата.
                    // Указывает на несоответствие или проблему в UserTaskService.StartTaskAsync при обработке уже начатых задач.
                    _logger.LogError("StartTask: Failed to re-confirm task {UserTaskId} which was already associated with chat {ChatId}. UserTaskService.StartTaskAsync returned null.", 
                        userTaskId, userTask.AssociatedChatId.Value);
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to confirm an already started task. Please try again or contact support.");
                 }
                 return Ok(currentDto);
            }

            // 2. Создаем или получаем чат для этой задачи
            Guid chatId;
            try
            {
                chatId = await _chatService.GetOrCreateProblemChatAsync(userTask.ProblemHash, userId, userTask.DisplayName, userTask.TaskType, HttpContext.RequestAborted);
            }
            catch (Exception chatEx)
            {
                 _logger.LogError(chatEx, "Failed to create or get chat for task {UserTaskId} (ProblemHash: {ProblemHash}) for user {UserId}", 
                    userTaskId, userTask.ProblemHash, userId);
                 return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create or retrieve the associated chat.");
            }

            if (chatId == Guid.Empty)
            { 
                _logger.LogError("ChatService returned an empty GUID for task {UserTaskId} (ProblemHash: {ProblemHash}) for user {UserId}", 
                    userTaskId, userTask.ProblemHash, userId);
                 return StatusCode(StatusCodes.Status500InternalServerError, "Failed to obtain a valid chat ID.");
            }
            
            // 3. Обновляем статус UserTask и сохраняем chatId
            var updatedTaskDto = await _userTaskService.StartTaskAsync(userTaskId, chatId, userId, HttpContext.RequestAborted);
            
            if (updatedTaskDto == null)
            {
                // Эта ветка теперь менее вероятна, т.к. основные проверки были выше
                _logger.LogWarning("Failed to update UserTask {UserTaskId} status after obtaining chat ID {ChatId} for user {UserId}.", 
                    userTaskId, chatId, userId);
                // Возможно, проблема с сохранением в StartTaskAsync?
                return NotFound("Task found and chat obtained, but failed to update task status."); 
            }
            
            // Убедимся, что возвращенный DTO содержит правильный chatId
            if (updatedTaskDto.AssociatedChatId != chatId)
            {
                 _logger.LogError("Mismatch! updatedTaskDto.AssociatedChatId ({DtoChatId}) != obtained chatId ({ChatId}) for UserTask {UserTaskId}", 
                    updatedTaskDto.AssociatedChatId, chatId, userTaskId);
                 // Возвращаем то, что получили, но логируем серьезную ошибку
            }
            
            return Ok(updatedTaskDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while starting user task {UserTaskId} for user {UserId}", userTaskId, userId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
        }
    }

    /// <summary>
    /// Помечает задачу как решенную (Solved).
    /// </summary>
    [HttpPost("{userTaskId:guid}/complete")]
    [ProducesResponseType(typeof(UserTaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteUserTask(Guid userTaskId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var completedDto = await _userTaskService.CompleteTaskAsync(userTaskId, userId, HttpContext.RequestAborted);
        if (completedDto == null)
        {
            return NotFound();
        }
        return Ok(completedDto);
    }
} 