namespace MathLLMBackend.Core.Configuration;

public class LlmServiceConfiguration
{
    public required string Token { get; init; }
    public required string Url { get; init; }
    public required string Model { get; init; }
}