namespace MathLLMBackend.Domain.Entities;

public class Identity
{
    public Identity(long userId, string email, string passwordHash)
    {
        UserId = userId;
        Email = email;
        PasswordHash = passwordHash;
    }


    public Identity() { }

    public long Id { get; set; }
    public long UserId { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
}
