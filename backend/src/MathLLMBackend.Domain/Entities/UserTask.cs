using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MathLLMBackend.Domain.Enums;

namespace MathLLMBackend.Domain.Entities;

public class UserTask
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string ApplicationUserId { get; set; } = null!;
    [ForeignKey(nameof(ApplicationUserId))]
    public ApplicationUser ApplicationUser { get; set; } = null!;

    [Required]
    [MaxLength(512)] // Идентификатор задачи, может быть длинным (например, из TASK_TYPES или хэш)
    public string ProblemId { get; set; } = null!;

    [Required]
    [MaxLength(1024)] // Название для отображения, может быть еще длиннее
    public string DisplayName { get; set; } = null!;

    // Тип задачи, например: 
    // 0 - Задачи из списка "Выбрать задачу" (инициализируются из TASK_TYPES)
    // 1 - Случайная задача (если будет такой режим)
    // 2 - Задачи из контрольной работы и т.д.
    [Required]
    public int TaskType { get; set; }

    [Required]
    public UserTaskStatus Status { get; set; }

    public Guid? AssociatedChatId { get; set; } // Nullable Guid, ID связанного чата
    // Если будет сущность Chat, можно добавить навигационное свойство:
    // public Chat? AssociatedChat { get; set; }
    // Но пока оставим только ID, чтобы не усложнять, если Chat не всегда связан напрямую или для гибкости

    // Дополнительные поля, если нужны:
    // public DateTime CreatedAt { get; set; }
    // public DateTime UpdatedAt { get; set; }

    public string ProblemHash { get; set; } = null!; // Хеш задачи из Geolin, добавлен для связи

    public UserTask()
    {
        Id = Guid.NewGuid();
        Status = UserTaskStatus.NotStarted;
        // CreatedAt = DateTime.UtcNow;
        // UpdatedAt = DateTime.UtcNow;
    }
} 