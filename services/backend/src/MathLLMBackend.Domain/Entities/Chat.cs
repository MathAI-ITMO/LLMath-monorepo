using MathLLMBackend.Domain.Enums;

namespace MathLLMBackend.Domain.Entities;

public class Chat
{
    public Chat(string name, string userId)
    {
        Name = name;
        UserId = userId;
    }

    public Chat() { }
    
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    public IEnumerable<Message> Messages { get; set; } = new List<Message>();
    public ChatType? Type { get; set; }
}
