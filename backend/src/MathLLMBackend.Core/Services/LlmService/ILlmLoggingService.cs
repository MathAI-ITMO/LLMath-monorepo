using MathLLMBackend.Domain.Entities;

namespace MathLLMBackend.Core.Services.LlmService;

/// <summary>
/// Сервис для логирования взаимодействий с LLM
/// </summary>
public interface ILlmLoggingService
{
    /// <summary>
    /// Логирует запрос к LLM и ответ от нее
    /// </summary>
    /// <param name="taskType">Тип задачи (режим)</param>
    /// <param name="messages">Сообщения, отправленные LLM</param>
    /// <param name="response">Ответ от LLM</param>
    /// <param name="modelName">Имя модели</param>
    Task LogInteraction(int taskType, IEnumerable<Message> messages, string response, string modelName);
    
    /// <summary>
    /// Логирует запрос на решение задачи и ответ от LLM
    /// </summary>
    /// <param name="problem">Условие задачи</param>
    /// <param name="solution">Решение от LLM</param>
    /// <param name="modelName">Имя модели</param>
    Task LogSolution(string problem, string solution, string modelName);
} 