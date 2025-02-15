namespace MathLLMBackend.Domain.Entities;

public class Chat
{
    public Chat(string? name, long userId)
    {
        Name = name;
        UserId = userId;
    }

    public Chat() { }

    public long UserId { get; set; }
    public string? Name { get; set; }

}
