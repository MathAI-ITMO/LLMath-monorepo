using System.Text;
using MathLLMBackend.Core.Services.LlmService;
using MathLLMBackend.DataAccess.Contexts;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MathLLMBackend.Core.Services.ChatService;

public class ChatService : IChatService
{
    private readonly AppDbContext _dbContext;
    private readonly ILlmService _llmService;

    public ChatService(AppDbContext dbContext, ILlmService llmService)
    {
        _dbContext = dbContext;
        _llmService = llmService;
    }
    
    public async Task<Chat> Create(Chat chat, CancellationToken ct)
    {
        var res = await _dbContext.Chats.AddAsync(chat, ct);
        await _dbContext.SaveChangesAsync(ct);
        return res.Entity;
    }
    
    public async Task<List<Chat>> GetUserChats(string userId, CancellationToken ct)
    {
        var chats = await _dbContext.Chats.Where(c => c.User.Id == userId).ToListAsync(cancellationToken: ct);
        await _dbContext.SaveChangesAsync(ct);
        return chats;
    }

    public async Task Delete(Chat chat, CancellationToken ct)
    {
        _dbContext.Chats.Remove(chat);
        await _dbContext.SaveChangesAsync(ct);
    }
    
    public async IAsyncEnumerable<string> CreateMessage(Message message, CancellationToken ct)
    {
        _dbContext.Messages.Add(message);
        var messages = await _dbContext.Messages.Where(m => m.Chat == message.Chat).ToListAsync(cancellationToken: ct);
        messages.Add(message);
        var text = _llmService.GenerateNextMessageText(messages);

        var fullText = new StringBuilder();
        await foreach (var messageText in text)
        {
            fullText.Append(messageText);
            yield return messageText;
        }
        
        var newMessage = new Message(message.Chat, fullText.ToString(), MessageType.Assistant);
        _dbContext.Messages.Add(newMessage);
        await _dbContext.SaveChangesAsync(ct);
    }
    
    public async Task<List<Message>> GetAllMessageFromChat(Chat chat, CancellationToken ct)
    {
        return await _dbContext.Messages.Where(m => m.Chat == chat).ToListAsync(ct);
    }

    public async Task<Chat?> GetChatById(Guid id, CancellationToken ct)
    {
        return await _dbContext.Chats
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken: ct);
    }

    public async Task<Message?> GetMessageId(Guid id, CancellationToken ct)
    {
        return await _dbContext.Messages.FirstOrDefaultAsync(c => c.Id == id, cancellationToken: ct);
    }
}
