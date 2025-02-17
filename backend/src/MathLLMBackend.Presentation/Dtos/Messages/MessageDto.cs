namespace MathLLMBackend.Presentation.Dtos.Messages;

public record MessageDto(long Id, long ChatId, string Text, string Type);
