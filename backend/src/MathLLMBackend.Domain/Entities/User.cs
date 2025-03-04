namespace MathLLMBackend.Domain.Entities;

public class User
{
    public User(string email, string firstName, string lastName)
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;
    }

    public User(long id, string email, string firstName, string lastName)
    {
        Id = id;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
    }

    public User() { }

    public long Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
