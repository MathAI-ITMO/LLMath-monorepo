namespace MathLLMBackend.Presentation.Models;

public class JwtUser
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string StudentGroup { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    public Guid UserId => Guid.TryParse(Id, out var guid) ? guid : Guid.Empty;
}
