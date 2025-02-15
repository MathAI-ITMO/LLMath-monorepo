using MathLLMBackend.Domain.Entities;

namespace MathLLMBackend.DomainServices.UserService;

public interface IUserService
{
    Task<User> Create(User user, string email, string password, CancellationToken ct);
    Task<User> GetById(long id, CancellationToken ct);
    Task<User> GetByEmail(string email, CancellationToken ct);
    Task<Identity> GetIdentity(User user, CancellationToken ct);
    Task<User> AuthenticateUser(string email, string password, CancellationToken ct);
}
