namespace MathLLMBackend.GeolinClient.Options;

public class GeolinClientOptions
{
    public required string BaseAddress { get; set; }
    public string? AuthorizationHeader { get; set; }
}