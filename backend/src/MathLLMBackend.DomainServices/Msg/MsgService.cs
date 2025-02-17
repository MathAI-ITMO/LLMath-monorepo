using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Domain.Exceptions;
using MathLLMBackend.Repository;

namespace MathLLMBackend.DomainServices.MessageService;

public class MessageService : IMessageService
{
    private readonly IMessagesRepository _messageRepository;
    private readonly IChatRepository _chatRepository;

    public MessageService(IMessagesRepository messageRepository, IChatRepository chatRepository)
    {
        _messageRepository = messageRepository;
        _chatRepository = chatRepository;
    }

    public async Task<Message> Create(Message message, long userId, CancellationToken ct)
    {
        if (!await _chatRepository.IsChatExistsForUser(userId, message.ChatId, ct))
            throw new AuthorizationException($"User {userId} does not have access to chat {message.ChatId}");

        var newMessage = await _messageRepository.Create(message, ct)
            ?? throw new InvalidOperationException("Unexpected error in Creating message"); 
        return newMessage;
    }

    public async Task<List<Message>> GetAllMessageFromChat(long userId, long chatId, CancellationToken ct)
    {
        if (!await _chatRepository.IsChatExistsForUser(userId, chatId, ct))
            throw new AuthorizationException($"User {userId} does not have access to chat {chatId}");

        var messages = await _messageRepository.GetAllMessageFromChat(chatId, ct)
            ?? throw new InvalidOperationException("Unexpected error in getting messages"); 
        
        return messages;
    }
}
