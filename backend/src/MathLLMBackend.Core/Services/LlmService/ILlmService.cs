using MathLLMBackend.Domain.Entities;

namespace MathLLMBackend.Core.Services.LlmService;

public interface ILlmService
{
    IAsyncEnumerable<string> GenerateNextMessageText(List<Message> messages);
}