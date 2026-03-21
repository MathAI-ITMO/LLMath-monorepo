using MathLLMBackend.Domain.Enums;

namespace MathLLMBackend.Core.Constants;

public static class TaskModeTitles
{
    public static readonly Dictionary<TaskType, string> Titles = new()
    {
        { TaskType.Learning, "Обучение на примерах" },
        { TaskType.Guided, "Наведение на мысль" },
        { TaskType.Exam, "Контрольная работа" }
    };
}
