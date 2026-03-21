namespace MathLLMBackend.Presentation.Dtos.Stats;

public class UserStatsDto
{
    public string UserId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string StudentGroup { get; set; } = string.Empty;
    public int SolvedCount { get; set; }
    public int InProgressCount { get; set; }
    public int NormalChatsCount { get; set; }
}
