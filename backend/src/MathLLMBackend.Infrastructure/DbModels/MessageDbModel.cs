using System;

namespace MathLLMBackend.Infrastructure.DbModels;

public class MessageDbModel
{
    public long Id { get; set; }
    public long ChatId { get; set; }
    public required string Text { get; set; }
    public DateTime CreatedAt { get; set; }
    public required string MessageType { get; set; } 
}
