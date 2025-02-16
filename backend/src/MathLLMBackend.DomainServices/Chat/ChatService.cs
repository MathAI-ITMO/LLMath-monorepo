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
        var newChat = await _chatRepository.Create(chat, ct)
        ?? throw new InvalidOperationException("Unexpected error in Creating message"); 
        return newChat;
    }
    public async Task<List<Chat>> GetAllChats(long userId, CancellationToken ct)
    {
        var chats = await _chatRepository.GetAllChats(userId, ct)
        ?? throw new InvalidOperationException("Unexpected error in getting chats"); 
        return chats;
    }
}
