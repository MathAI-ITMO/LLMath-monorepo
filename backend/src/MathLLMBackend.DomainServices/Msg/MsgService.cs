using System.ClientModel;
using System.Diagnostics;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Domain.Enums;
using MathLLMBackend.Domain.Exceptions;
using MathLLMBackend.Repository;
using OpenAI;
using OpenAI.Chat;

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
        
        var messages = await _messageRepository.GetAllMessageFromChat(message.ChatId, ct)
            ?? throw new InvalidOperationException("Unexpected error in getting messages"); 

        var client = new ChatClient (
            model: Environment.GetEnvironmentVariable("OPENAI_MODEL"),
            credential: new ApiKeyCredential(Environment.GetEnvironmentVariable("OPENAI_API_KEY")),
            options: new OpenAIClientOptions() { Endpoint =  new Uri(Environment.GetEnvironmentVariable("OPENAI_API_ENDPOINT"))} );

        var openaiMessages = messages.Select<Message, ChatMessage>(m =>
            m.MessageType switch
            {
                MessageType.User => new UserChatMessage(m.Text),
                MessageType.Assistant => new AssistantChatMessage(m.Text),
                MessageType.System => new SystemChatMessage(m.Text),
                _ => throw new NotImplementedException()
            }
        );

        ChatCompletion completion = client.CompleteChat(openaiMessages);

        var resultMessage = new Message(message.ChatId, completion.Content[0].Text, MessageType.Assistant);

        await _messageRepository.Create(resultMessage, ct);
        return resultMessage;
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
