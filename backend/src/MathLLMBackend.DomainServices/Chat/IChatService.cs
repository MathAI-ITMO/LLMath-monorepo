using MathLLMBackend.Domain.Entities;

namespace MathLLMBackend.DomainServices.ChatService;

public interface IChatService
{
    Task<Chat> Create(Chat chat, CancellationToken ct);
}
