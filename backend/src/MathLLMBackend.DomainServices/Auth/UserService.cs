using System.Transactions;
using MathLLMBackend.Domain.Entities;
using MathLLMBackend.Domain.Exceptions;
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
            throw new AuthorizationException("Invalid credentials");
        }

        var user = await _usersRepository.Get(identity.UserId, ct)
            ?? throw new AuthorizationException("Invalid credentials");

        return user;
    }

    public async Task<User> Create(User user, string email, string password, CancellationToken ct)
    {
        var salt = BCrypt.Net.BCrypt.GenerateSalt(12);
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, salt);

        using var scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);

        var newUser = await _usersRepository.Create(user, ct) 
            ?? throw new InvalidOperationException("User with the same  number already exists");

        var identity = await _identityRepository.Create(new Identity(newUser.Id, email, hashedPassword), ct)
            ?? throw new InvalidOperationException("User with the same email already exists");

        scope.Complete();

        return newUser;
    }

    public async Task<User> GetByEmail(string email, CancellationToken ct)
    {
        var identity = await _identityRepository.GetByEmail(email, ct)
            ?? throw new InvalidOperationException("User not found");

        var user = await _usersRepository.Get(identity.UserId, ct);
        return user!;
    }

    public async Task<User> GetById(long id, CancellationToken ct)
    {
        var user = await _usersRepository.Get(id, ct)
            ?? throw new InvalidOperationException("User not found");   
        return user;
    }

    public async Task<Identity> GetIdentity(User user, CancellationToken ct)
    {
        var identity = await _identityRepository.GetByUserId(user.Id, ct);
        return identity;
    }
}
