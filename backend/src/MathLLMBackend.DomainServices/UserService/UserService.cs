using System.Transactions;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Domain.Exceptions;
using MathLLMBackend.Repository;

namespace MathLLMBackend.DomainServices.UserService;

public class UserService : IUserService
{
    private const int PasswordConstFactor = 12;

    private readonly IIdentityRepository _identityRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly ISessionRepository _sessionRepository;

    public UserService(IIdentityRepository identityRepository, IUsersRepository usersRepository, ISessionRepository sessionRepository)
    {
        _identityRepository = identityRepository;
        _usersRepository = usersRepository;
        _sessionRepository = sessionRepository;
    }

    public async Task AddUser(User user, string email, string password, CancellationToken ct)
    {
        var salt = BCrypt.Net.BCrypt.GenerateSalt(PasswordConstFactor);
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, salt);

        using var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);

        var newUser = await _usersRepository.CreateUser(user, ct);
        await _identityRepository.Create(new Identity(newUser.Id, email, hashedPassword), ct);

        scope.Complete();
    }

    public async Task<Session> CreateSession(string email, string password, CancellationToken ct)
    {
        var identity = await _identityRepository.GetByEmail(email, ct);

        if (identity is null || !BCrypt.Net.BCrypt.CheckPassword(password, identity.PasswordHash))
            throw new InvalidPasswordException();

        var user = await _usersRepository.GetUser(identity.UserId, ct);
        var session = await _sessionRepository.Create(user, ct);
        return session;

    }

    public async Task<User?> GetUser(Guid token, CancellationToken ct)
    {
        var session = await _sessionRepository.Get(token, ct);
        var userId = session.UserId;
        var user = await _usersRepository.GetUser(userId, ct);
        return user;
    }

    public async Task<User> GetUser(string email, string password, CancellationToken ct)
    {
        var identity = await _identityRepository.GetByEmail(email, ct);

        if (identity is null || !BCrypt.Net.BCrypt.CheckPassword(password, identity.PasswordHash))
            throw new InvalidPasswordException();

        var user = await _usersRepository.GetUser(identity.UserId, ct);
        return user;
    }
}
