using MathLLMBackend.Domain.Entities;

namespace MathLLMBackend.DomainServices.MessageService;

public interface IMessageService
{
    Task<Message> Create(Message message, long userId, CancellationToken ct);
    Task<List<Message>> GetAllMessageFromChat(long userId, long chatId, CancellationToken ct);
}
