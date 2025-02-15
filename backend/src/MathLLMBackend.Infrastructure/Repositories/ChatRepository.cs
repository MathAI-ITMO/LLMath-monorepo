using System.Transactions;
using Dapper;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Repository;

namespace MathLLMBackend.Infrastructure.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly DataContext _context;

        public ChatRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Chat?> Create(Chat chat, CancellationToken ct)
        {
            const string chatSql =
            """
            insert into chats(name, user_id)
            values(@Name, @UserId)
            on conflict do nothing
            returning 
                id as Id
              , name as Name
              , user_id as UserId;
            """;

            using var conn = _context.CreateConnection();

            var createdChatCommand = new CommandDefinition(chatSql,
            new
            {
                Name = chat.Name,
                UserId = chat.UserId
            },
            cancellationToken: ct);

            var createdChat = await conn.QuerySingleOrDefaultAsync<Chat>(createdChatCommand);
            return createdChat;
        }
    }
}