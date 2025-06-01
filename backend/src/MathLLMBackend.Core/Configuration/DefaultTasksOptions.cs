namespace MathLLMBackend.Core.Configuration;

public class DefaultTasksOptions
{
    public const string SectionName = "DefaultTasks";

    public List<string> Type0 { get; init; } = new();
    public List<string> Type1 { get; init; } = new();
    public List<string> Type2 { get; init; } = new();
    public List<string> Type3 { get; init; } = new();
    // При необходимости можно добавить другие типы задач:
    // public List<string> Type1 { get; init; } = new();
} 