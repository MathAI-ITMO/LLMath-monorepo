using Dapper;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Infrastructure.Converters;
using MathLLMBackend.Infrastructure.DbModels;
using MathLLMBackend.Repository;

namespace MathLLMBackend.Infrastructure.Repositories
{
    public class MessageRepository : IMessagesRepository
    {
        private readonly DataContext _context;

        public MessageRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Message?> Create(Message message, CancellationToken ct)
        {
            const string messageSql =
            """
            insert into chat_messages(chat_id, text, message_type)
            values(@ChatId, @Message, @Type::message_type)
            on conflict do nothing
            returning 
                id as Id,
                chat_id as ChatId,
                created_at as CreatedAt,
                text as Text,
                message_type::text as MessageType;
            """;

            using var conn = _context.CreateConnection();

            var createdMessageCommand = new CommandDefinition(messageSql,
            new
            {
                ChatId = message.ChatId,
                Message = message.Text,
                Type = message.MessageType.ToDbName()
            },
            cancellationToken: ct);
            var createdMessage = await conn.QuerySingleOrDefaultAsync<MessageDbModel>(createdMessageCommand);
            return createdMessage?.ToDomain();
        }

        public async Task<List<Message>?> GetAllMessageFromChat(long chatId, CancellationToken ct)
        {
            const string messageSql =
            """
            select 
                id as Id
              , chat_id as ChatId 
              ,  text as Text
              ,  message_type::text as MessageType
              ,  created_at as CreatedAt from chat_messages
            where chat_id=@ChatId
            order by created_at;
            """;

            using var conn = _context.CreateConnection();

            var createdMessageCommand = new CommandDefinition(messageSql,
            new
            {
                ChatId = chatId
            },
            cancellationToken: ct);

            var messages = await conn.QueryAsync<MessageDbModel>(createdMessageCommand);
            return [.. messages.Select(m => m.ToDomain())];
        }
    }
}