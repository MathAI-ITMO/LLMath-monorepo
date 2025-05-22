using System.Text.Json.Serialization;

namespace MathLLMBackend.ProblemsClient.Models;

public class AdditionalProperty
{

}

public class ProblemStep
{
    [JsonPropertyName("order")]
    public int Order { get; set; }

    [JsonPropertyName("prerequisites")]
    public AdditionalProperty Prerequisites {get; set;} = new();

    [JsonPropertyName("transition")]
    public AdditionalProperty Transition {get; set;} = new();

    [JsonPropertyName("outcomes")]
    public AdditionalProperty Outcomes {get; set;} = new();
}

public class GeolinKey
{
    [JsonPropertyName("hash")]
    public string Hash { get; set; } = string.Empty;

    [JsonPropertyName("seed")]
    public int Seed { get; set; }
}

public class Solution
{
    [JsonPropertyName("steps")]
    public List<ProblemStep> Steps { get; set; } = new();
}

public class Problem
{
    [JsonPropertyName("_id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("statement")]
    public string Statement { get; set; } = string.Empty;

    [JsonPropertyName("llm_solution")]
    public object? LlmSolution { get; set; }

    [JsonPropertyName("geolin_ans_key")]
    public GeolinKey GeolinAnsKey { get; set; } = new();

    [JsonPropertyName("result")]
    public string Result { get; set; } = string.Empty;

    [JsonPropertyName("solution")]
    public Solution Solution { get; set; } = new();
}
