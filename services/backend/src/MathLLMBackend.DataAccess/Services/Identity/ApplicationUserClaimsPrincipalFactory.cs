using System.Security.Claims;
using MathLLMBackend.Domain.Constants;
using MathLLMBackend.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace MathLLMBackend.DataAccess.Services.Identity;

public class ApplicationUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    public ApplicationUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> options)
        : base(userManager, roleManager, options)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        var roles = await UserManager.GetRolesAsync(user);

        foreach (var role in roles)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        identity.AddClaim(new Claim(ClaimTypeConstants.FirstName, user.FirstName ?? string.Empty));
        identity.AddClaim(new Claim(ClaimTypeConstants.LastName, user.LastName ?? string.Empty));
        identity.AddClaim(new Claim(ClaimTypeConstants.StudentGroup, user.StudentGroup ?? string.Empty));

        if (identity.FindFirst(ClaimTypes.Email) == null && !string.IsNullOrEmpty(user.Email))
        {
            identity.AddClaim(new Claim(ClaimTypes.Email, user.Email));
        }

        return identity;
    }
}
