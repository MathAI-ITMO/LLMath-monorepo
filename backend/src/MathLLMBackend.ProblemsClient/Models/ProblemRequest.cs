using System.Text.Json.Serialization;
namespace MathLLMBackend.ProblemsClient.Models;


public class ProblemRequest
{
    [JsonPropertyName("statement")]
    public string Statement { get; set; } = string.Empty;

    [JsonPropertyName("geolin_ans_key")]
    public GeolinKey GeolinAnsKey { get; set; } = new();
    
    [JsonPropertyName("result")]
    public string Result { get; set; } = string.Empty;

    [JsonPropertyName("solution")]
    public Solution Solution { get; set; } = new();
}
