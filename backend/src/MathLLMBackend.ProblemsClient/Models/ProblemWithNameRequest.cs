using System.Text.Json.Serialization;

namespace MathLLMBackend.ProblemsClient.Models;

public class ProblemWithNameRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("problem_id")]
    public string ProblemId { get; set; } = string.Empty;
}
