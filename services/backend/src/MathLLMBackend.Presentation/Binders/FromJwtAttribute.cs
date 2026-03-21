using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MathLLMBackend.Presentation.Binders;

[AttributeUsage(AttributeTargets.Parameter)]
public class FromJwtAttribute : Attribute, IBindingSourceMetadata
{
    public BindingSource BindingSource => BindingSource.Custom;
}
