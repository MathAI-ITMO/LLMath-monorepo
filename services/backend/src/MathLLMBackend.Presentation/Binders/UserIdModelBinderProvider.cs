using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Reflection;
using MathLLMBackend.Presentation.Models;

namespace MathLLMBackend.Presentation.Binders;

public class UserIdModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (context.Metadata.BindingSource == BindingSource.Custom)
        {
            var parameterInfo = context.Metadata as Microsoft.AspNetCore.Mvc.ModelBinding.Metadata.DefaultModelMetadata;
            if (parameterInfo?.Attributes?.Attributes != null)
            {
                foreach (var attribute in parameterInfo.Attributes.Attributes)
                {
                    if (attribute is FromJwtAttribute)
                    {
                        if (context.Metadata.ModelType == typeof(JwtUser))
                        {
                            return new JwtUserModelBinder();
                        }
                    }
                }
            }
        }

        return null;
    }
}
