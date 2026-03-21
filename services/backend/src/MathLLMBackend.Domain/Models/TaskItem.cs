using MathLLMBackend.Domain.Enums;

namespace MathLLMBackend.Domain.Models;

public class TaskItem
{
    public Guid UserTaskId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public Guid? ChatId { get; set; }
    public TaskType TaskType { get; set; }
}
