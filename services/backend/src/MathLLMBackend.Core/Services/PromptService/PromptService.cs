using MathLLMBackend.Core.Configuration;
using MathLLMBackend.Domain.Enums;
using Microsoft.Extensions.Options;

namespace MathLLMBackend.Core.Services.PromptService;

public class PromptService(IOptions<PromptConfiguration> promptConfiguration) : IPromptService
{
    private readonly PromptConfiguration _promptConfiguration = promptConfiguration.Value;

    public string GetTutorSystemPrompt()
    {
        return _promptConfiguration.TutorSystemPrompt;
    }

    public string GetTutorSolutionPrompt(string solution)
    {
        return _promptConfiguration.TutorSolutionPrompt.Replace("{solution}", solution);
    }

    public string GetSolverSystemPrompt()
    {
        return _promptConfiguration.SolverSystemPrompt;
    }

    public string GetSolverTaskPrompt(string task)
    {
        return _promptConfiguration.SolverTaskPrompt.Replace("{problem}", task);
    }

    public string GetDefaultSystemPrompt()
    {
        return _promptConfiguration.DefaultSystemPrompt;
    }
    
    public string GetLearningSystemPrompt()
    {
        return _promptConfiguration.LearningSystemPrompt;
    }
    
    public string GetGuidedSystemPrompt()
    {
        return _promptConfiguration.GuidedSystemPrompt;
    }
    
    public string GetExamSystemPrompt()
    {
        return _promptConfiguration.ExamSystemPrompt;
    }
    
    public string GetSystemPromptByTaskType(TaskType taskType)
    {
        return taskType switch
        {
            TaskType.Learning => GetLearningSystemPrompt(),
            TaskType.Guided => GetGuidedSystemPrompt(),
            TaskType.Exam => GetExamSystemPrompt(),
            _ => GetTutorSystemPrompt()
        };
    }
    
    public string GetTutorInitialPrompt()
    {
        return _promptConfiguration.TutorInitialPrompt;
    }
    
    public string GetLearningInitialPrompt(string condition, string firstStep)
    {
        // TODO: Consider using condition and firstStep parameters if prompt template supports placeholders
        return _promptConfiguration.LearningInitialPrompt;
    }
    
    public string GetGuidedInitialPrompt()
    {
        return _promptConfiguration.GuidedInitialPrompt;
    }
    
    public string GetExamInitialPrompt()
    {
        return _promptConfiguration.ExamInitialPrompt;
    }
    
    public string GetInitialPromptByTaskType(TaskType taskType, string condition, string firstStep)
    {
        return taskType switch
        {
            TaskType.Learning => GetLearningInitialPrompt(condition, firstStep),
            TaskType.Guided => GetGuidedInitialPrompt(),
            TaskType.Exam => GetExamInitialPrompt(),
            _ => GetTutorInitialPrompt()
        };
    }

    public string GetExtractAnswerSystemPrompt()
    {
        return _promptConfiguration.ExtractAnswerSystemPrompt;
    }

    public string GetExtractAnswerPrompt(string problemStatement, string solution)
    {
        return _promptConfiguration.ExtractAnswerPrompt
            .Replace("{problemStatement}", problemStatement)
            .Replace("{solution}", solution);
    }
} 