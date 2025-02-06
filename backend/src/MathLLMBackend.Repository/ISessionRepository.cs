using System;
using MathLLMBackend.Domain.Entities;

namespace MathLLMBackend.Repository;

public interface ISessionRepository
{
    Task<Session> Create(User user, CancellationToken ct);
    Task<Session> Get(Guid Token, CancellationToken ct);
}
