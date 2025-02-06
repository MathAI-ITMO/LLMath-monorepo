namespace MathLLMBackend.Domain.Entities;

public class Session
{
    public Session() {}

    public long Id { get; set; }
    public long UserId { get; set; }
    public Guid Token { get; set; }
}
