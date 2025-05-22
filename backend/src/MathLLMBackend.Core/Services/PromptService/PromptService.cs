using MathLLMBackend.Core.Configuration;
using Microsoft.Extensions.Options;

namespace MathLLMBackend.Core.Services.PromptService;

public class PromptService : IPromptService
{
    private readonly PromptConfiguration _promptConfiguration;

    public PromptService(IOptions<PromptConfiguration> promptConfiguration)
    {
        _promptConfiguration = promptConfiguration.Value;
    }

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
    
    public string GetSystemPromptByTaskType(int taskType)
    {
        return taskType switch
        {
            1 => GetLearningSystemPrompt(),
            2 => GetGuidedSystemPrompt(),
            3 => GetExamSystemPrompt(),
            _ => GetTutorSystemPrompt() // используем TutorSystemPrompt как стандартный для обычных задач
        };
    }
    
    public string GetTutorInitialPrompt()
    {
        return _promptConfiguration.TutorInitialPrompt;
    }
    
    public string GetLearningInitialPrompt(string condition, string firstStep)
    {
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
    
    public string GetInitialPromptByTaskType(int taskType, string condition, string firstStep)
    {
        return taskType switch
        {
            1 => GetLearningInitialPrompt(condition, firstStep),
            2 => GetGuidedInitialPrompt(),
            3 => GetExamInitialPrompt(),
            _ => GetTutorInitialPrompt() // используем TutorInitialPrompt как стандартный для обычных задач
        };
    }
} 