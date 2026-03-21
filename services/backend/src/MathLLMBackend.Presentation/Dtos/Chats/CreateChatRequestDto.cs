namespace MathLLMBackend.Presentation.Dtos.Chats;

public record CreateChatRequestDto(string Name, Guid? ProblemId);
