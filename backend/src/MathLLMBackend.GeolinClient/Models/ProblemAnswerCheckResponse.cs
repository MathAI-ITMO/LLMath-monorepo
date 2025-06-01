using System.Text.Json.Serialization;

namespace MathLLMBackend.GeolinClient.Models;

public class ProblemAnswerCheckResponse
{
    [JsonPropertyName("verdict")]
    public double Verdict { get; set; }
} 