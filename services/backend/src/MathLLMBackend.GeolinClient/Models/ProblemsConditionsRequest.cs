using System.Text.Json.Serialization;

namespace MathLLMBackend.GeolinClient.Models;

public class ProblemsConditionsRequest
{
    [JsonPropertyName("problems")]
    public List<ProblemConditionRequest> Problems { get; set; } = new();
} 