using System.Security.Claims;
using System.Text.Encodings.Web;
using MathLLMBackend.Domain.Constants;
using MathLLMBackend.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MathLLMBackend.IntegrationTests;

public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Test-User-Id", out var userIdHeader))
        {
            return AuthenticateResult.NoResult();
        }

        var userId = userIdHeader.ToString();
        if (string.IsNullOrEmpty(userId))
        {
            return AuthenticateResult.NoResult();
        }

        var serviceProvider = Context.RequestServices;
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            var fallbackClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, "test@example.com"),
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim(ClaimTypeConstants.FirstName, "Test"),
                new Claim(ClaimTypeConstants.LastName, "User"),
                new Claim(ClaimTypeConstants.StudentGroup, "TestGroup"),
                new Claim(ClaimTypes.Role, Role.User)
            };

            var fallbackIdentity = new ClaimsIdentity(fallbackClaims, "Test");
            var fallbackPrincipal = new ClaimsPrincipal(fallbackIdentity);
            return AuthenticateResult.Success(new AuthenticationTicket(fallbackPrincipal, "Test"));
        }

        var roles = await userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypeConstants.FirstName, user.FirstName ?? string.Empty),
            new Claim(ClaimTypeConstants.LastName, user.LastName ?? string.Empty),
            new Claim(ClaimTypeConstants.StudentGroup, user.StudentGroup ?? string.Empty)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return AuthenticateResult.Success(ticket);
    }
}
