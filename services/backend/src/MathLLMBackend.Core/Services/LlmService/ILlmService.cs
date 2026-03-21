using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Domain.Enums;

namespace MathLLMBackend.Core.Services.LlmService;

public interface ILlmService
{
    Task<string> SolveProblem(string problemDescription, CancellationToken ct);
    Task<string> GenerateNextMessageAsync(List<Message> messages, TaskType taskType, CancellationToken ct);
    Task<string> ExtractAnswer(string problemStatement, string solution, CancellationToken ct);
}