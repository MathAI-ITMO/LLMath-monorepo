namespace MathLLMBackend.Presentation.Dtos.Messages;

public record MessageDto(Guid Id, Guid ChatId, string Text, string? Type, DateTime? CreationTime);
