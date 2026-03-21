using MathLLMBackend.Domain.Enums;

namespace MathLLMBackend.Domain.Models;

public record ProblemUpdateModel(
    string Title,
    string Statement,
    string LlmSolution,
    string? TheoryLink,
    string? GeolinHash,
    long? GeolinSeed,
    IEnumerable<TaskType> Types
);
