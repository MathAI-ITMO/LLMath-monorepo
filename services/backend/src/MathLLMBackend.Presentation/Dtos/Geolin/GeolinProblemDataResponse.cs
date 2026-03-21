using System.Text.Json.Serialization;

namespace MathLLMBackend.Presentation.Dtos.Geolin;

public class GeolinProblemDataResponse
{
    public string? Name { get; set; }
    public string? Hash { get; set; }
    public string? Condition { get; set; }
    public int? Seed { get; set; }
    public string? Error { get; set; }
    public string? ProblemParams { get; set; }
}
