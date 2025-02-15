using MathLLMBackend.Domain.Entities;

namespace MathLLMBackend.Repository;

public interface IChatRepository
{
    Task<Chat?> Create(Chat chat, CancellationToken ct);
    //Task<Chat?> Get(long Id, CancellationToken ct);
}
