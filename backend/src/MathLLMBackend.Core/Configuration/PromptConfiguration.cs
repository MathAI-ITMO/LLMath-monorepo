namespace MathLLMBackend.Core.Configuration;

public class PromptConfiguration
{
    public required string TutorSystemPrompt { get; set; }
    public required string TutorSolutionPrompt { get; set; }
    public required string SolverSystemPrompt { get; set; }
    public required string SolverTaskPrompt { get; set; }
    public required string DefaultSystemPrompt { get; set; }
}