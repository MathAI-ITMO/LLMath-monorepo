namespace MathLLMBackend.Presentation.Dtos.Auth;

public record RegisterDto(string FirstName, string LastName, string StudentGroup, string Email, string Password);
