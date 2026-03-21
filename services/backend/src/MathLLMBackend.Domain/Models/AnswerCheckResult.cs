namespace MathLLMBackend.Domain.Models;

public class AnswerCheckResult
{
    public bool IsCorrect { get; set; }
    public double Verdict { get; set; }
}
