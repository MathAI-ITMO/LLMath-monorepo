using MathLLMBackend.Domain.Enums;

namespace MathLLMBackend.Core.Dtos; // Изменено пространство имен

public record UserTaskDto(
    Guid Id,
    string ProblemId, // Идентификатор задачи
    string DisplayName, // Название для отображения
    int TaskType,
    UserTaskStatus Status,
    Guid? AssociatedChatId
); 