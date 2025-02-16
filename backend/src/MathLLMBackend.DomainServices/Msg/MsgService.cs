using System.Transactions;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Domain.Exceptions;
using MathLLMBackend.Repository;

namespace MathLLMBackend.DomainServices.MsgService;

public class MsgService : IMsgService
{
    private readonly IMsgRepository _msgRepository;

    public MsgService(IMsgRepository msgRepository)
    {
        _msgRepository = msgRepository;
    }

    public async Task<Msg> Create(Msg msg, long chatId, CancellationToken ct)
    {
        var newMsg = await _msgRepository.Create(msg, chatId, ct)
        ?? throw new InvalidOperationException("Unexpected error in Creating chat"); 
        return newMsg;
    }

    public async Task<List<Msg>> GetAllMsgFromChat(long chatId, CancellationToken ct)
    {
        var msgs = await _msgRepository.GetAllMsgFromChat(chatId, ct)
        ?? throw new InvalidOperationException("Unexpected error in getting chats"); 
        return msgs;
    }
}
