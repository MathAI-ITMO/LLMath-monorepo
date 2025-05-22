using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Core.Dtos;

namespace MathLLMBackend.Core.Services;

public interface IUserTaskService
{
    /// <summary>
    /// Получает задачи пользователя указанного типа.
    /// Если задачи не существуют, инициализирует их на основе конфигурации по умолчанию.
    /// </summary>
    /// <param name="userId">ID пользователя.</param>
    /// <param name="taskType">Тип задач.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Список задач пользователя.</returns>
    Task<IEnumerable<UserTaskDto>> GetOrCreateUserTasksAsync(string userId, int taskType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновляет статус задачи пользователя на InProgress и связывает ее с чатом.
    /// </summary>
    /// <param name="userTaskId">ID задачи пользователя.</param>
    /// <param name="chatId">ID созданного чата.</param>
    /// <param name="userId">ID пользователя (для проверки владения).</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Обновленная задача пользователя или null, если задача не найдена или пользователь не владелец.</returns>
    Task<UserTaskDto?> StartTaskAsync(Guid userTaskId, Guid chatId, string userId, CancellationToken cancellationToken = default);

    Task<UserTask?> GetUserTaskByIdAsync(Guid userTaskId, string userId, CancellationToken cancellationToken = default);

    Task<UserTaskDto?> CompleteTaskAsync(Guid userTaskId, string userId, CancellationToken cancellationToken = default);
} 