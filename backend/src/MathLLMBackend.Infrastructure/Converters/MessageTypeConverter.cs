using MathLLMBackend.Domain.Enums;

namespace MathLLMBackend.Infrastructure.Converters;

public static class MessageTypeConverter
{
    public static MessageType ToMessageType(string message)
    {
        return message switch
        {
            "user" => MessageType.User,
            "system" => MessageType.System,
            "assistant" => MessageType.Assistant,
            _ => throw new ArgumentException("Invalid message type", nameof(message)),
        };
    }

    public static string ToDbName(this MessageType messageType)
    {
        return messageType switch
        {
            MessageType.User => "user",
            MessageType.System => "system",
            MessageType.Assistant => "assistant",
            _ => throw new ArgumentException("Invalid message type", nameof(messageType)),
        };
    }
}
