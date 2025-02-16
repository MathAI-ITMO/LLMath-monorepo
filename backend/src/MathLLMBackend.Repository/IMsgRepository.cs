using MathLLMBackend.Domain.Entities;

namespace MathLLMBackend.Repository;

public interface IMsgRepository
{
    Task<Msg?> Create(Msg msg, long chatId, CancellationToken ct);
}
