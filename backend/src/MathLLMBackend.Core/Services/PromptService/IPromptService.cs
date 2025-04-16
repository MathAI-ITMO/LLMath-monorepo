namespace MathLLMBackend.Core.Services.PromptService;

public interface IPromptService
{
    string GetTutorSystemPrompt();
    string GetTutorSolutionPrompt(string solution);
    string GetSolverSystemPrompt();
    string GetSolverTaskPrompt(string task);
    string GetDefaultSystemPrompt();
}