namespace MathLLMBackend.Domain.Entities;

public class User
{
    public User(string? firstName, string? lastName, long? isuId)
    {
        FirstName = firstName;
        LastName = lastName;
        IsuId = isuId;
    }

    public User() { }

    public long Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public long? IsuId { get; set; }

}
