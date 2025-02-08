using Dapper;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Repository;

namespace MathLLMBackend.Infrastructure.Repositories;

public class IdentityRepository : IIdentityRepository
{
    private readonly DataContext _context;

    public IdentityRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<Identity?> Create(Identity identity, CancellationToken ct)
    {
        const string sql =
        """
        insert into identities (user_id, email, password_hash) 
        values (@UserId, @Email, @PasswordHash)
        on conflict do nothing
        returning
            id as Id
          , user_id as UserId
          , email as Email
          , password_hash as PasswordHash
        """;

        using var conn = _context.CreateConnection();
        conn.Open();
        var cmd = new CommandDefinition(
            sql,
            new
            {
                UserId = identity.UserId,
                Email = identity.Email,
                PasswordHash = identity.PasswordHash
            },
            cancellationToken: ct
        );

        return await conn.QuerySingleOrDefaultAsync<Identity>(cmd);

    }

    public async Task<Identity?> GetByEmail(string email, CancellationToken ct)
    {
        const string sql =
        """
        select 
            id as Id
          , user_id as UserId
          , email as Email
          , password_hash as PasswordHash
        from identities
        where email = @Email
        """;

        using var conn = _context.CreateConnection();
        conn.Open();
        var cmd = new CommandDefinition(
            sql,
            new
            {
                Email = email,
            },
            cancellationToken: ct
        );

        return await conn.QuerySingleOrDefaultAsync<Identity>(cmd);
    }

    public async Task<Identity> GetByUserId(long userId, CancellationToken ct)
    {
        const string sql =
        """
        select 
            id as Id
          , user_id as UserId
          , email as Email
          , password_hash as PasswordHash
        from identities
        where user_id = @UserId
        """;

        using var conn = _context.CreateConnection();
        conn.Open();
        var cmd = new CommandDefinition(
            sql,
            new
            {
                UserId = userId
            },
            cancellationToken: ct
        );

        return await conn.QuerySingleAsync<Identity>(cmd);
    }
}
