using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Text.Json.Nodes;

namespace MathLLMBackend.Presentation.Configuration;

public class EnumSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;
        var enumType = Nullable.GetUnderlyingType(type) ?? type;
        if (!enumType.IsEnum) return Task.CompletedTask;

        schema.Type = JsonSchemaType.String;
        schema.Enum = Enum.GetNames(enumType)
            .Select(n => (JsonNode)JsonValue.Create(n)!)
            .ToList();

        return Task.CompletedTask;
    }
}
