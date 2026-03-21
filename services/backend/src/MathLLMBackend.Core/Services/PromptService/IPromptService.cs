using MathLLMBackend.Domain.Enums;

namespace MathLLMBackend.Core.Services.PromptService;

public interface IPromptService
{
    string GetTutorSolutionPrompt(string solution);
    string GetSolverSystemPrompt();
    string GetSolverTaskPrompt(string task);
    string GetDefaultSystemPrompt();
    string GetLearningSystemPrompt();
    string GetGuidedSystemPrompt();
    string GetExamSystemPrompt();
    string GetSystemPromptByTaskType(TaskType taskType);
    string GetInitialPromptByTaskType(TaskType taskType, string condition, string firstStep);
    string GetExtractAnswerSystemPrompt();
    string GetExtractAnswerPrompt(string problemStatement, string solution);
}