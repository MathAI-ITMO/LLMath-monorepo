using MathLLMBackend.Domain.Entities;

namespace MathLLMBackend.Repository;

public interface IChatRepository
{
    Task<Chat?> Create(Chat chat, CancellationToken ct);
    Task<List<Chat>?> GetAllChats(long userId, CancellationToken ct);
}
