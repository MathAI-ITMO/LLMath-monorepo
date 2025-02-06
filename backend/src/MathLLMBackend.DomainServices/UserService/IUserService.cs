using System;
using MathLLMBackend.Domain.Entities;

namespace MathLLMBackend.DomainServices.UserService;

public interface IUserService
{
    Task AddUser(User user, string email, string password, CancellationToken ct);
    Task<User?> GetUser(Guid token, CancellationToken ct);
    Task<Session> CreateSession(string email, string password, CancellationToken ct);
}
