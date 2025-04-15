using MathLLMBackend.Domain.Enums;
using Microsoft.AspNetCore.Identity;

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
    public string Name { get; set; }
    public string UserId { get; set; }
    public IdentityUser User { get; set; }
    public IEnumerable<Message> Messages { get; set; }
    public ChatType? Type { get; set; }
}
