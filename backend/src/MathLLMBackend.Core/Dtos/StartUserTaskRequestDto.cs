using System.ComponentModel.DataAnnotations;

namespace MathLLMBackend.Core.Dtos; // Изменено пространство имен

public record StartUserTaskRequestDto(
    [Required] Guid ChatId
); 