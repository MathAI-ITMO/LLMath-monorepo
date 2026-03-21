namespace MathLLMBackend.Presentation.Dtos.Tasks;

public record ProblemDto(
    string Hash,
    string Name,
    string Description,
    string Condition,
    int Seed,
    string Params
); 