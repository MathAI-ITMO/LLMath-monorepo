using MathLLMBackend.Domain.Entities;

namespace MathLLMBackend.DomainServices.MsgService;

public interface IMsgService
{
    Task<Msg> Create(Msg msg, long chatId, CancellationToken ct);
}
