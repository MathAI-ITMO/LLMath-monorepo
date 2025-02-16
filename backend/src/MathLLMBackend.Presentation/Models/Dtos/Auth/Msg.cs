using MathLLMBackend.Domain.Entities;

namespace MathLLMBackend.Presentation.Models.Dtos;

public record MsgDto(string Text, long chatId);
