using System.Security.Claims;
using MathLLMBackend.DomainServices.UserService;

namespace MathLLMBackend.Presentation.Middlewares;

public class AuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly UserService _userService;

    public AuthorizationMiddleware(RequestDelegate next, UserService userService)
    {
        _next = next;
        _userService = userService;

    }

    public async Task Invoke(HttpContext context, CancellationToken ct)
    {
        if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader) ||
            !authHeader.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var tokenString = authHeader.ToString().Substring("Bearer ".Length).Trim();

        if (!Guid.TryParse(tokenString, out var token))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var user = await _userService.GetUser(token, ct);
        if (user is null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }


        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, token.ToString())
        };

        context.Items["User"] = user;

        await _next(context);
    }

}
