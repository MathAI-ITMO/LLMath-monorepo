namespace MathLLMBackend.Presentation.Dtos.Tasks;

public record ProblemsPageDto(
    List<ProblemDto> Problems,
    int Number 
); 