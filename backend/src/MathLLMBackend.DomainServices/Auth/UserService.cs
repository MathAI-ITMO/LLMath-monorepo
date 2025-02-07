using System.Transactions;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Repository;

namespace MathLLMBackend.DomainServices.UserService;

public class UserService : IUserService
{
    private readonly IIdentityRepository _identityRepository;
    private readonly IUsersRepository _usersRepository;

    public UserService(IIdentityRepository identityRepository, IUsersRepository usersRepository)
    {
        _identityRepository = identityRepository;
        _usersRepository = usersRepository;
    }

    public async Task<User> AuthenticateUser(string email, string password, CancellationToken ct)
    {
        var identity = await _identityRepository.GetByEmail(email, ct);

        if (identity == null || !BCrypt.Net.BCrypt.CheckPassword(password, identity.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        var user = await _usersRepository.Get(identity.UserId, ct)
            ?? throw new UnauthorizedAccessException("Invalid credentials");

        return user;
    }

    public async Task<User> Create(User user, string email, string password, CancellationToken ct)
    {
        var salt = BCrypt.Net.BCrypt.GenerateSalt(12);
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, salt);

        using var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);

        var newUser = await _usersRepository.Create(user, ct);
        await _identityRepository.Create(new Identity(newUser.Id, email, hashedPassword), ct);

        scope.Complete();

        return newUser;
    }

    public async Task<User?> GetByEmail(string email, CancellationToken ct)
    {
        var identity = await _identityRepository.GetByEmail(email, ct);

        if (identity is null) return null;
        var user = await _usersRepository.Get(identity.UserId, ct);
        return user;
    }

    public async Task<User?> GetById(long id, CancellationToken ct)
    {
        var user = await _usersRepository.Get(id, ct);
        return user;
    }

    public async Task<Identity> GetIdentity(User user, CancellationToken ct)
    {
        var identity = await _identityRepository.GetByUserId(user.Id, ct);
        return identity;
    }
}
