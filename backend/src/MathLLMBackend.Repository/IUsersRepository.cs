using MathLLMBackend.Domain.Entities;

namespace MathLLMBackend.Repository;

public interface IUsersRepository
{
    Task<User> Create(User user, CancellationToken ct);
    Task<User> Get(long Id, CancellationToken ct);
}
