using System.Text.Json.Serialization;

namespace MathLLMBackend.GeolinClient.Models;

public class ProblemAnswerCheckRequest
{
    [JsonPropertyName("hash")]
    public string Hash { get; set; } = string.Empty;

    [JsonPropertyName("seed")]
    public int? Seed { get; set; }

    [JsonPropertyName("problem_params")]
    public string? ProblemParams { get; set; }

    [JsonPropertyName("answer_attempt")]
    public string AnswerAttempt { get; set; } = string.Empty;
} 