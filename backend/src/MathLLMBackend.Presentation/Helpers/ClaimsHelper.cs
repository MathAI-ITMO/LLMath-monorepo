using System;
using System.Security.Claims;
using System.Security.Principal;

namespace MathLLMBackend.Presentation.Helpers;

public static class ClaimsHelper
{
    public static long GetUserId(this ClaimsPrincipal principal)
    {
        return long.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
