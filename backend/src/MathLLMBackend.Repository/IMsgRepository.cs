using MathLLMBackend.Domain.Entities;

namespace MathLLMBackend.Repository;

public interface IMsgRepository
{
    Task<Msg?> Create(Msg msg, long chatId, CancellationToken ct);
    Task<List<Msg>?> GetAllMsgFromChat(long chatId, CancellationToken ct);
}
