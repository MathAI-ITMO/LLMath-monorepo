using MathLLMBackend.Domain.Entities;

namespace MathLLMBackend.Core.Services.AuthService;

public interface IAuthService
{
    Task<ApplicationUser> RegisterAsync(string email, string password, string firstName, string lastName, string studentGroup, CancellationToken ct = default);
}
