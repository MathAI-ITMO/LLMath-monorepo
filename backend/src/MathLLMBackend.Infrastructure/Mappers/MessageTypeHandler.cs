using Dapper;
using MathLLMBackend.Domain.Enums;
using System;
using System.Data;

namespace MathLLMBackend.Infrastructure.Mappers;

public class MessageTypeHandler : SqlMapper.TypeHandler<MessageType>
{
    public override MessageType Parse(object value)
    {
        throw new NotImplementedException();
    }

    public override void SetValue(IDbDataParameter parameter, MessageType value)
    {
        
    }
}