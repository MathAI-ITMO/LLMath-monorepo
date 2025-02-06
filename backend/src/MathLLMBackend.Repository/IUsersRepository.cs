using MathLLMBackend.Domain.Entities;

namespace MathLLMBackend.Repository;

public interface IUsersRepository
{
    Task<User> CreateUser(User user, CancellationToken ct);
    Task<User> GetUser(long Id, CancellationToken ct);
}
