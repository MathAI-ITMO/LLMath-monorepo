using MathLLMBackend.Domain.Entities;

namespace MathLLMBackend.DomainServices.MsgService;

public interface IMsgService
{
    Task<Msg> Create(Msg msg, long chatId, CancellationToken ct);
    Task<List<Msg>> GetAllMsgFromChat(long chatId, CancellationToken ct);
}
