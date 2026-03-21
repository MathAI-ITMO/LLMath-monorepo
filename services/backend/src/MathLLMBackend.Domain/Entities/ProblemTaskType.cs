using MathLLMBackend.Domain.Enums;

namespace MathLLMBackend.Domain.Entities;

public class ProblemTaskType
{
    public ProblemTaskType(Problem problem, TaskType taskType)
    {
        ProblemId = problem.Id;
        Problem = problem;
        TaskType = taskType;
    }

    public ProblemTaskType() { }

    public Guid ProblemId { get; set; }
    public Problem Problem { get; set; } = null!;
    public TaskType TaskType { get; set; }
}