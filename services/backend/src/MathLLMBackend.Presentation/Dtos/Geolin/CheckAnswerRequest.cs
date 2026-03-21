namespace MathLLMBackend.Presentation.Dtos.Geolin;

public class CheckAnswerRequest
{
    public required string Hash { get; set; }
    public required string AnswerAttempt { get; set; }
    public int Seed { get; set; }
    public required string ProblemParams { get; set; }
}
