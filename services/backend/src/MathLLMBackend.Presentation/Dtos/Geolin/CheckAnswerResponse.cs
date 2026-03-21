namespace MathLLMBackend.Presentation.Dtos.Geolin;

public class CheckAnswerResponse
{
    public bool IsCorrect { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
    public string Hash { get; set; } = "";
    public string AnswerAttempt { get; set; } = "";
    public int? Seed { get; set; }
}
