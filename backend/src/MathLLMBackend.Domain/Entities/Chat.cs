namespace MathLLMBackend.Domain.Entities;

public class Chat
{
    public Chat(string name, long? userId)
    {
        Name = name;
        UserId = userId;
    }

    public Chat() { }
    
    public long Id { get; set; }
    public long? UserId { get; set; }
    public long? user_id { get {return UserId;} set{UserId=value;} }
    public string Name { get; set; }

}
