using System;
using MathLLMBackend.Domain.Entities;

namespace MathLLMBackend.Repository;

public interface IIdentityRepository
{
    Task<Identity> GetByUserId(long userId, CancellationToken ct);
    Task<Identity?> GetByEmail(string email, CancellationToken ct);
    Task<Identity?> Create(Identity identity, CancellationToken ct);

}
