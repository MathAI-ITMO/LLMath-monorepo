using System.Text.Json.Serialization;

namespace MathLLMBackend.GeolinClient.Models;

public class ProblemInfoRequest
{
    [JsonPropertyName("hashes")]
    public List<string> Hashes { get; set; } = new();
} 