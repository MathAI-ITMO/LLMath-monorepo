namespace MathLLMBackend.GeolinClient.Options;

public class GeolinClientOptions
{
    public string BaseAddress { get; set; } = null!;
    public string? AuthorizationHeader { get; set; }
}