using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Domain.Enums;

namespace MathLLMBackend.Core.Services.LlmService;

public interface ILlmService
{
    IAsyncEnumerable<string> GenerateNextMessageStreaming(List<Message> messages, CancellationToken ct);
    Task<string> SolveProblem(string message, CancellationToken ct);
    Task<string> GenerateNextMessageAsync(List<Message> messages, CancellationToken ct);
}