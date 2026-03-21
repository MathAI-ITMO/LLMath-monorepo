namespace MathLLMBackend.Presentation.Dtos.Llm;

public class ExtractAnswerRequest
{
    public string ProblemStatement { get; set; } = "";
    public string Solution { get; set; } = "";
}
