namespace MathLLMBackend.Presentation.Dtos.Stats;

public class UserDetailDto
{
    public List<TaskItemDto> SolvedTasks { get; set; } = new();
    public List<TaskItemDto> InProgressTasks { get; set; } = new();
    public List<ChatItemDto> Chats { get; set; } = new();
}
