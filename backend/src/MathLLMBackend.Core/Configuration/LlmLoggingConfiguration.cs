namespace MathLLMBackend.Core.Configuration;

/// <summary>
/// Конфигурация для логирования взаимодействий с LLM
/// </summary>
public class LlmLoggingConfiguration
{
    /// <summary>
    /// Включено ли логирование
    /// </summary>
    public bool Enabled { get; set; }
    
    /// <summary>
    /// Путь к файлу логов
    /// </summary>
    public string LogFilePath { get; set; } = "logs/llm_interactions.log";
} 