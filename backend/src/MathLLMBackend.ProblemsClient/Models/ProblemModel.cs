using System.Text.Json.Serialization;

namespace MathLLMBackend.ProblemsClient.Models;

public class AbstractProperty
{

}
public class AdditionalProperty
{
    [JsonPropertyName("additionalProp1")]
    public AbstractProperty AdditionalProp1 { get; set;} = new();
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
    public string _hash { get; set; } = string.Empty;

    [JsonPropertyName("seed")]
    public int _seed { get; set; }
}

public class Solution
{
    [JsonPropertyName("steps")]
    public List<ProblemStep> _steps { get; set; } = new();
}

public class Problem
{
    [JsonPropertyName("_id")]
    public string? _id { get; set; }

    [JsonPropertyName("statement")]
    public string _statement { get; set; } = string.Empty;

    [JsonPropertyName("geolin_ans_key")]
    public GeolinKey _geolinAnsKey { get; set; } = new();

    [JsonPropertyName("result")]
    public string _result { get; set; } = string.Empty;

    [JsonPropertyName("solution")]
    public Solution _solution { get; set; } = new();
}
