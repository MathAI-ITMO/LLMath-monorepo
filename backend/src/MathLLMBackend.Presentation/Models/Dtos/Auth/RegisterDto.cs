namespace MathLLMBackend.Presentation.Models.Dtos;

public record RegisterDto(string? FirstName, string? LastName, long? IsuId, string Email, string Password);
