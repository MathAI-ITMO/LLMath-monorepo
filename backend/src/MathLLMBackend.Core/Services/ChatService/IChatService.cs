using System.Text;
using MathLLMBackend.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace MathLLMBackend.Core.Services.ChatService;

public interface IChatService
{
    Task<Chat> Create(Chat chat, CancellationToken ct);
    Task<Chat> Create(Chat chat, string problemHash, int explicitTaskType, CancellationToken ct);
    Task<Guid> GetOrCreateProblemChatAsync(string problemHash, string userId, string taskDisplayName, int taskType, CancellationToken ct);
    Task Delete(Chat chat, CancellationToken ct);
    Task<List<Chat>> GetUserChats(string userId, CancellationToken ct);
    IAsyncEnumerable<string> CreateMessage(Message message, CancellationToken ct);
    public Task<List<Message>> GetAllMessageFromChat(Chat chat, CancellationToken ct);
    public Task<Chat?> GetChatById(Guid id, CancellationToken ct);
    public Task<Message?> GetMessageId(Guid id, CancellationToken ct);
}
