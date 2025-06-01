namespace MathLLMBackend.Core.Configuration;

public class PromptConfiguration
{
    public required string TutorSystemPrompt { get; set; }
    public required string TutorSolutionPrompt { get; set; }
    public required string SolverSystemPrompt { get; set; }
    public required string SolverTaskPrompt { get; set; }
    public required string DefaultSystemPrompt { get; set; }
    public required string LearningSystemPrompt { get; set; }
    public required string GuidedSystemPrompt { get; set; }
    public required string ExamSystemPrompt { get; set; }
    public required string TutorInitialPrompt { get; set; }
    public required string LearningInitialPrompt { get; set; }
    public required string GuidedInitialPrompt { get; set; }
    public required string ExamInitialPrompt { get; set; }
    public required string ExtractAnswerSystemPrompt { get; set; }
    public required string ExtractAnswerPrompt { get; set; }
}