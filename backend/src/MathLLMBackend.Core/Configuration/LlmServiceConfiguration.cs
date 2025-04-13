namespace MathLLMBackend.Core.Configuration;

public class LlmServiceConfiguration
{
    public required ModelConfiguration ChatModel { get; init; }
    public required ModelConfiguration SolverModel { get; init; }
    
    public class ModelConfiguration
    {
        public required string Token { get; init; }
        public required string Url { get; init; }
        public required string Model { get; init; }
    }
}