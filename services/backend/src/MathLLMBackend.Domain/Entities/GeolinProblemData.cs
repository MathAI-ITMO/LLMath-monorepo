namespace MathLLMBackend.Domain.Entities;

public class GeolinProblemData
{
    public GeolinProblemData(Guid problemId, string hash, long seed)
    {
        Id = Guid.NewGuid();
        ProblemId = problemId;
        Hash = hash;
        Seed = seed;
    }

    public GeolinProblemData() { }

    public Guid Id { get; set; }
    public Guid ProblemId { get; set; }
    public string Hash { get; set; } = null!;
    public long Seed { get; set; }
}