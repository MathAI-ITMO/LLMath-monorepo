using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace MathLLMBackend.Presentation.Configuration;

public class FromUserIdOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var userIdParameters = context.Description.ParameterDescriptions
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

        return Task.CompletedTask;
    }
}
