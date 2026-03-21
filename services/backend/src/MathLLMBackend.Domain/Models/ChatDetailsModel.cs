using MathLLMBackend.Domain.Enums;

namespace MathLLMBackend.Domain.Models;

public record ChatDetailsModel(TaskType? TaskType, string? TheoryLink);
