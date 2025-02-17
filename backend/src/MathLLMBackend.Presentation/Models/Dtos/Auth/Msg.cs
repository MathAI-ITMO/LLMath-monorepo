using MathLLMBackend.Domain.Entities;

namespace MathLLMBackend.Presentation.Models.Dtos;

public record MessageDto(string Text, long chatId);
