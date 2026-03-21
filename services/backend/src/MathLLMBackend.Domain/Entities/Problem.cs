using MathLLMBackend.Domain.Enums;

namespace MathLLMBackend.Domain.Entities;

public class Problem
{
    public Problem(string llmSolution, string statement, string title)
    {
        Id = Guid.NewGuid();
        LlmSolution = llmSolution;
        Statement = statement;
        Title = title;
    }
    
    public Problem() { }

    public Guid Id { get; set; }
    public string LlmSolution { get; set; } = null!;
    public string Statement { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? TheoryLink { get; set; }
    public GeolinProblemData GeolinProblemData { get; set; } = null!;
    public virtual ICollection<ProblemTaskType> Types { get; set; } = new List<ProblemTaskType>();
}