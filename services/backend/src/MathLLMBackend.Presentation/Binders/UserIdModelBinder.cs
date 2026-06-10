using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MathLLMBackend.Presentation.Binders;

public class UserIdModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        var userId = bindingContext.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            bindingContext.ModelState.TryAddModelError(
                bindingContext.ModelName,
                "User ID not found in token. This should not happen with [Authorize] attribute.");
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        bindingContext.Result = ModelBindingResult.Success(userId);
        return Task.CompletedTask;
    }
}
