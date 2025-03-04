using System;
using System.Security.Claims;
using System.Security.Principal;
using MathLLMBackend.Domain.Entities;

namespace MathLLMBackend.Presentation.Helpers;

public static class ClaimsHelper
{
    public static long GetUserId(this ClaimsPrincipal principal)
    {
        return long.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    public static User GetUser(this ClaimsPrincipal principal)
    {
        return new User
        (
            principal.GetUserId(),
            principal.FindFirstValue(ClaimTypes.Email)!,
            principal.FindFirstValue("FirstName")!,
            principal.FindFirstValue("LastName")!
        );
    }
}
