using MathLLMBackend.Domain.Enums;

namespace MathLLMBackend.Presentation.Dtos.Tasks;

public record UpdateProblemRequestDto(
    string Title,
    string Statement,
    string LlmSolution,
    string? TheoryLink,
    string? GeolinHash,
    long? GeolinSeed,
    IEnumerable<TaskType> Types
);
