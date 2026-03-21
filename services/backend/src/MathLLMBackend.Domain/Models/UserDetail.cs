namespace MathLLMBackend.Domain.Models;

public class UserDetail
{
    public List<TaskItem> SolvedTasks { get; set; } = new();
    public List<TaskItem> InProgressTasks { get; set; } = new();
    public List<ChatItem> Chats { get; set; } = new();
}
