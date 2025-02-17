using MathLLMBackend.Domain.Enums;

namespace MathLLMBackend.Domain.Entities;

public class Message
{
    public Message(long chatId, string text, MessageType type)
    {
        ChatId = chatId;
        Text = text;
        MessageType = type;
        
    }
    public Message() { }

    public long Id { get; set; }
    public long ChatId { get; set; }
    public string Text { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public  MessageType MessageType { get; set; }
}
