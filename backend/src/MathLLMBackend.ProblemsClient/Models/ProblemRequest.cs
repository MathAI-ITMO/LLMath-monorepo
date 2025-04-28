using System.Text.Json.Serialization;
namespace MathLLMBackend.ProblemsClient.Models;


public class ProblemRequest
{
    [JsonPropertyName("statement")]
    public string _statement { get; set; } = string.Empty;

    [JsonPropertyName("geolin_ans_key")]
    public GeolinKey _geolinAnsKey { get; set; } = new();
    
    [JsonPropertyName("result")]
    public string _result { get; set; } = string.Empty;

    [JsonPropertyName("solution")]
    public Solution _solution { get; set; } = new();
}
