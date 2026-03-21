using MathLLMBackend.Domain.Constants;
using MathLLMBackend.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace MathLLMBackend.Core.Services.AuthService;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AuthService> _logger;

    public AuthService(UserManager<ApplicationUser> userManager, ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<ApplicationUser> RegisterAsync(string email, string password, string firstName, string lastName, string studentGroup, CancellationToken ct = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("Пользователь с таким email уже существует");
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            StudentGroup = studentGroup
        };

        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, Role.User);
            _logger.LogInformation("User created a new account with extended profile.");
            return user;
        }

        var errorMessages = TranslateIdentityErrors(result.Errors);
        var combinedMessage = string.Join("; ", errorMessages);
        
        throw new InvalidOperationException($"Не удалось создать аккаунт: {combinedMessage}");
    }

    private static List<string> TranslateIdentityErrors(IEnumerable<IdentityError> errors)
    {
        var messages = new List<string>();
        
        foreach (var error in errors)
        {
            string message;
            
            if (error.Code == "DuplicateUserName" || error.Code == "DuplicateEmail")
            {
                message = "Пользователь с таким email уже существует";
            }
            else if (error.Code.StartsWith("Password"))
            {
                message = "Пароль не соответствует требованиям безопасности. Используйте буквы разного регистра, цифры и специальные символы";
            }
            else
            {
                message = error.Description;
            }
            
            messages.Add(message);
        }
        
        return messages;
    }
}
