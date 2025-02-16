using System.Transactions;
using Dapper;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Repository;

namespace MathLLMBackend.Infrastructure.Repositories
{
    public class MsgRepository : IMsgRepository
    {
        private readonly DataContext _context;

        public MsgRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Msg?> Create(Msg msg, long chatId, CancellationToken ct)
        {
            const string msgSql =
            """
            insert into chat_messages(chat_id, message, message_type)
            values(@ChatId, @Message, 'user')
            on conflict do nothing
            returning 
                id as Id,
                chat_id as ChatId,
                created_at as CreatedAt,
                message as Message,
                message_type as MType;
            """;

            using var conn = _context.CreateConnection();

            var createdMsgCommand = new CommandDefinition(msgSql,
            new
            {
                ChatId = chatId,
                Message = msg.Message
            },
            cancellationToken: ct);
            var createdMsg = await conn.QuerySingleOrDefaultAsync<Msg>(createdMsgCommand);
            return createdMsg;
        }
    }
}