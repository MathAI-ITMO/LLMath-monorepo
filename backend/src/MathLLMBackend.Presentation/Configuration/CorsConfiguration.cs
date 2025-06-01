namespace MathLLMBackend.Presentation.Configuration;

public class CorsConfiguration
{
    public bool Enabled { get; set; }
    public string Origin { get; set; } = string.Empty;
}