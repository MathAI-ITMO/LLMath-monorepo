using MathLLMBackend.Presentation.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MathLLMBackend.Presentation.Configuration;

public class FromUserIdOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var userIdParameters = context.ApiDescription.ParameterDescriptions
            .Where(p => p.Source == BindingSource.Custom)
            .Select(p => p.Name);

        foreach (var name in userIdParameters)
        {
            var parameter = operation.Parameters.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (parameter != null)
            {
                operation.Parameters.Remove(parameter);
            }
        }
    }
}
