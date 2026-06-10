using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Domain.Enums;

namespace MathLLMBackend.Core.Services;

public interface IUserTaskService
{
    Task<IEnumerable<UserTask>> GetOrCreateUserTasksAsync(string userId, TaskType taskType, CancellationToken cancellationToken = default);
    Task<UserTask> StartUserTaskWithChatAsync(Guid userTaskId, string userId, CancellationToken cancellationToken = default);
    Task<UserTask?> CompleteTaskAsync(Guid userTaskId, string userId, CancellationToken cancellationToken = default);
}