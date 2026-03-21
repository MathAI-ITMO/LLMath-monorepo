using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Domain.Enums;
using MathLLMBackend.Domain.Models;

namespace MathLLMBackend.Core.Services.ChatService;

public interface IChatService
{
    Task<Chat> Create(Chat chat, CancellationToken ct);
    Task<Chat> Create(Chat chat, Guid problemId, TaskType explicitTaskType, CancellationToken ct);
    Task<Guid> GetOrCreateProblemChatAsync(Guid problemId, string userId, string taskDisplayName, TaskType taskType, CancellationToken ct);
    Task Delete(Guid chatId, string userId, CancellationToken ct);
    Task<List<Chat>> GetUserChats(string userId, CancellationToken ct);
    Task<string> CreateMessageForUser(Guid chatId, string userId, string text, CancellationToken ct);
    Task<List<Message>> GetUserVisibleMessagesFromChat(Guid chatId, string userId, CancellationToken ct);
    Task<Chat> GetChatByIdForUser(Guid chatId, string userId, CancellationToken ct);
    Task<ChatDetailsModel> GetChatDetailsAsync(Guid chatId, string userId, CancellationToken ct);
    Task<Chat?> GetChatByIdAsync(Guid chatId, CancellationToken ct);
    Task<List<Message>> GetUserVisibleMessagesFromChatForAdmin(Guid chatId, CancellationToken ct);
    Task<ChatDetailsModel> GetChatDetailsForAdminAsync(Guid chatId, CancellationToken ct);
}

