using System.Text.Json.Serialization;

namespace MathLLMBackend.GeolinClient.Models;

public class ProblemPageResponse
{
    [JsonPropertyName("problems")]
    public List<ProblemInfoResponse> Problems { get; set; } = new();

    [JsonPropertyName("number")]
    public int Number { get; set; }
} 