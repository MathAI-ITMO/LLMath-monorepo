namespace MathLLMBackend.Domain.Enums;

public enum UserTaskStatus
{
    // NotStarted, // Удаляем дубликат
    // InProgress, // Удаляем дубликат
    // Solved, // Возможно, понадобится более гранулярное разделение, например, SolvedCorrectly, SolvedIncorrectly
    // Attempted // "Attempted" может быть синонимом InProgress или Solved.
    // Пока оставим так, потом можно будет расширить.
    // Предлагаю пока: NotStarted, InProgress, Completed (означает, что попытка была, результат не важен для текущей логики)
    // Или Finished - чтобы не путать с Completed из других контекстов.
    // Давайте остановимся на: NotStarted, InProgress, Submitted (пользователь отправил на проверку/завершил попытку)
    // По условию задачи: NotStarted, InProgress, Solved, Attempted. 
    // Solved - решена (успешно), Attempted - была попытка (не обязательно успешная, или просто открывал)
    // Если чат создан - это InProgress. Если пользователь как-то завершил работу с задачей - это может быть Solved/Attempted.
    // Пока что, если есть чат - это InProgress. Solved/Attempted - это следующий этап.
    // Для текущей задачи: NotStarted, InProgress. Solved/Attempted оставим для будущих улучшений.
    // По задаче: "статус становится 'в процессе'", "Если в будущем он опять нажмет на этот же пункт списка - у него должен открыться уже существующий чат".
    // Для текущей логики достаточно NotStarted и InProgress.
    // Однако, в ТЗ есть "<статус решения>". Это подразумевает больше состояний.
    // Возьмем изначальный вариант: NotStarted, InProgress, Solved, Attempted
    NotStarted = 0,
    InProgress = 1,
    Solved = 2,     // Задача успешно решена
    Attempted = 3   // Была попытка решения, но не обязательно успешная (или просто открыта и закрыта без решения)
} 