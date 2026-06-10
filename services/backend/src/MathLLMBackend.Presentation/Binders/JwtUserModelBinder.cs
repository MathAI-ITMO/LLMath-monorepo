using System.Security.Claims;
using MathLLMBackend.Domain.Constants;
using MathLLMBackend.Presentation.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MathLLMBackend.Presentation.Binders;

public class JwtUserModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var user = bindingContext.HttpContext.User;

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            bindingContext.ModelState.TryAddModelError(
                bindingContext.ModelName,
                "User ID not found in token. This should not happen with [Authorize] attribute.");
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        var jwtUser = new JwtUser
        {
            Id = userId,
            Email = user.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            FirstName = user.FindFirstValue(ClaimTypeConstants.FirstName) ?? string.Empty,
            LastName = user.FindFirstValue(ClaimTypeConstants.LastName) ?? string.Empty,
            StudentGroup = user.FindFirstValue(ClaimTypeConstants.StudentGroup) ?? string.Empty,
            Role = user.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .FirstOrDefault() ?? string.Empty
        };

        bindingContext.Result = ModelBindingResult.Success(jwtUser);
        return Task.CompletedTask;
    }
}
