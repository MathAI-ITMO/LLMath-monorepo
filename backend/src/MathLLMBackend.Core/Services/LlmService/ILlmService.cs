using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Domain.Enums;

namespace MathLLMBackend.Core.Services.LlmService;

public interface ILlmService
{
    IAsyncEnumerable<string> GenerateNextMessageStreaming(List<Message> messages, int taskType, CancellationToken ct);
    Task<string> SolveProblem(string problemDescription, CancellationToken ct);
    Task<string> GenerateNextMessageAsync(List<Message> messages, int taskType, CancellationToken ct);
}