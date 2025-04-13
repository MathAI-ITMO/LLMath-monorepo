using System.Text.Json.Serialization;

namespace MathLLMBackend.GeolinClient.Models;

public class ProblemConditionResponse
{
    [JsonPropertyName("problem_params")]
    public string ProblemParams { get; set; } = string.Empty;

    [JsonPropertyName("condition")]
    public string Condition { get; set; } = string.Empty;
} 