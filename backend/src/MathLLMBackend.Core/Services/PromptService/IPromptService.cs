namespace MathLLMBackend.Core.Services.PromptService;

public interface IPromptService
{
    string GetTutorSystemPrompt();
    string GetTutorSolutionPrompt(string solution);
    string GetSolverSystemPrompt();
    string GetSolverTaskPrompt(string task);
    string GetDefaultSystemPrompt();
    string GetLearningSystemPrompt();
    string GetGuidedSystemPrompt();
    string GetExamSystemPrompt();
    string GetSystemPromptByTaskType(int taskType);
    
    string GetTutorInitialPrompt();
    string GetLearningInitialPrompt(string condition, string firstStep);
    string GetGuidedInitialPrompt();
    string GetExamInitialPrompt();
    string GetInitialPromptByTaskType(int taskType, string condition, string firstStep);
    
    string GetExtractAnswerSystemPrompt();
    string GetExtractAnswerPrompt(string problemStatement, string solution);
}