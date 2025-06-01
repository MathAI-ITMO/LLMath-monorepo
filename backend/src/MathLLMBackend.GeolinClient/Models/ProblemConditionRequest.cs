using System.Text.Json.Serialization;

namespace MathLLMBackend.GeolinClient.Models;

public class ProblemConditionRequest
{
    [JsonPropertyName("hash")]
    public string Hash { get; set; } = string.Empty;

    [JsonPropertyName("seed")]
    public int? Seed { get; set; }

    [JsonPropertyName("lang")]
    public string Lang { get; set; } = string.Empty;
} 