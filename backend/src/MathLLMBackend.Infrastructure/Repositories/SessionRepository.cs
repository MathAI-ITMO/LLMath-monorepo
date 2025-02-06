using System;
using Dapper;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Repository;

namespace MathLLMBackend.Infrastructure.Repositories;

public class SessionRepository : ISessionRepository
{
    private readonly DataContext _context;

    public SessionRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<Session> Create(User user, CancellationToken ct)
    {
        const string sql =
        """
        insert into 
        sessions (user_id) 
        values (@UserId)
        returning 
              id as Id
            , user_id as UserId
            , token as Token
        """;

        using var conn = _context.CreateConnection();
        conn.Open();
        var command = new CommandDefinition(sql, new { UserId = user.Id }, cancellationToken: ct);
        return await conn.QuerySingleAsync<Session>(command);
    }

    public async Task<Session> Get(Guid Token, CancellationToken ct)
    {
        const string sql =
        """
        select 
              id as Id
            , user_id as UserId
            , token as Token
        from sessions 
        where token = @Token
        """;
        using var conn = _context.CreateConnection();
        conn.Open();
        var command = new CommandDefinition(sql, new { Token = Token }, cancellationToken: ct);
        return await conn.QuerySingleAsync<Session>(command);
    }
}
