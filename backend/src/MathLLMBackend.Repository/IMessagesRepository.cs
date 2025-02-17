using MathLLMBackend.Domain.Entities;

namespace MathLLMBackend.Repository;

public interface IMessagesRepository
{
    Task<Message?> Create(Message message, CancellationToken ct);
    Task<List<Message>?> GetAllMessageFromChat(long chatId, CancellationToken ct);
}
