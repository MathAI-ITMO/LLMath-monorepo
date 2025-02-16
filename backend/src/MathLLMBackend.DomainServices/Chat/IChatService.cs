using MathLLMBackend.Domain.Entities;

namespace MathLLMBackend.DomainServices.ChatService;

public interface IChatService
{
    Task<Chat> Create(Chat chat, CancellationToken ct);
    Task<List<Chat>> GetAllChats(long userId, CancellationToken ct);
    
}
