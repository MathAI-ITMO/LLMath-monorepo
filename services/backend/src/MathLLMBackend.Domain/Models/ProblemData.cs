namespace MathLLMBackend.Domain.Models;

public class ProblemData
{
    public required string Name { get; set; }
    public required string Hash { get; set; }
    public required string Condition { get; set; }
    public required string? Description { get; set; }
    public int Seed { get; set; }
    public string? ProblemParams { get; set; }
}
