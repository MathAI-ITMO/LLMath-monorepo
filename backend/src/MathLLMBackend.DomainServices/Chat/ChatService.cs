using System.Transactions;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Domain.Exceptions;
using MathLLMBackend.Repository;

namespace MathLLMBackend.DomainServices.ChatService;

public class ChatService : IChatService
{
    private readonly IChatRepository _chatRepository;

    public ChatService(IChatRepository chatRepository)
    {
        _chatRepository = chatRepository;
    }


    public async Task<Chat> Create(Chat chat, CancellationToken ct)
    {
        using var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
        var newChat = await _chatRepository.Create(chat, ct)
        ?? throw new InvalidOperationException("Unexpected error in Creating chat"); 
        scope.Complete();
        return newChat;
    }
}
