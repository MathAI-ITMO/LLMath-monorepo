using MathLLMBackend.Domain.Enums;

namespace MathLLMBackend.Presentation.Dtos.Chats;

public record ChatDto(Guid Id, string Name, string Type, TaskType? TaskType, string? TheoryLink);
