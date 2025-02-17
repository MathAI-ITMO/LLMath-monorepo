namespace MathLLMBackend.Domain.Entities;

public class User
{
    public User(string? firstName, string? lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public User() { }

    public long Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
