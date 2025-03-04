using Dapper;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Repository;

namespace MathLLMBackend.Infrastructure.Repositories
{
    public class UserRepository : IUsersRepository
    {
        private readonly DataContext _context;

        public UserRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<User?> Create(User user, CancellationToken ct)
        {
            const string userSql =
            """
            insert into users(email, first_name, last_name)
            values(@Email, @FirstName, @LastName)
            on conflict do nothing
            returning 
                id as Id
              , email as Email
              , first_name as FirstName
              , last_name as LastName
            """;

            using var conn = _context.CreateConnection();

            var createdUserCommand = new CommandDefinition(userSql,
            new
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
            },
            cancellationToken: ct);

            var createdUser = await conn.QuerySingleOrDefaultAsync<User>(createdUserCommand);
            return createdUser;
        }

        public async Task<User?> Get(long Id, CancellationToken ct)
        {
            const string sql =
            """
            select 
                id as Id
              , email as Email
              , first_name as FirstName
              , last_name as LastName
            from users
            where id = @Id;
            """;

            using var conn = _context.CreateConnection();
            var command = new CommandDefinition(sql,
            new
            {
                Id = Id
            },
            cancellationToken: ct);

            return await conn.QuerySingleOrDefaultAsync<User>(command);
        }
    }
}