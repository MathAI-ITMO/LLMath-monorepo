using System.Text.Json.Serialization;

namespace MathLLMBackend.GeolinClient.Models;

public class ProblemsConditionsResponse
{
    [JsonPropertyName("conditions")]
    public List<ProblemConditionResponse> Conditions { get; set; } = new();
} 