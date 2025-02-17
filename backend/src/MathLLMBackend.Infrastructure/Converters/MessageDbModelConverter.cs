using System;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Infrastructure.DbModels;

namespace MathLLMBackend.Infrastructure.Converters;

public static class MessageDbModelConverter
{
    public static Message ToDomain(this MessageDbModel dbModel)
        {
            var messageType = MessageTypeConverter.ToMessageType(dbModel.MessageType);

            return new Message
            {
                Id = dbModel.Id,
                ChatId = dbModel.ChatId,
                Text = dbModel.Text,
                CreatedAt = dbModel.CreatedAt,
                MessageType = messageType
            };
        }
}
