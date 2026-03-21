using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MathLLMBackend.Domain.Enums;

namespace MathLLMBackend.Domain.Entities;

public class UserTask
{
    public UserTask()
    {
        Id = Guid.NewGuid();
        Status = UserTaskStatus.NotStarted;
    }

    public Guid Id { get; set; }
    public string ApplicationUserId { get; set; } = null!;
    public ApplicationUser ApplicationUser { get; set; } = null!;
    public Guid ProblemId { get; set; }
    public Problem? Problem { get; set; }
    public string DisplayName { get; set; } = null!;
    public TaskType TaskType { get; set; }
    public ProblemTaskType ProblemTaskType { get; set; } = null!;
    public UserTaskStatus Status { get; set; }
    public Guid? AssociatedChatId { get; set; }
    public string ProblemHash { get; set; } = null!;
} 