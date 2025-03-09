using Microsoft.AspNetCore.Identity;

namespace MathLLMBackend.Domain.Entities;

public class Chat
{
    public Chat(string name, IdentityUser user)
    {
        Name = name;
        User = user;
    }

    public Chat() { }
    
    public Guid ChatId { get; set; }
    public string Name { get; set; }
    public IdentityUser User { get; set; }
    
    public IEnumerable<Message> Messages { get; set; }
}
