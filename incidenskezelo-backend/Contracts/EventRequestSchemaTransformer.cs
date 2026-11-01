using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace incidenskezelo_backend.Contracts;

public class EventRequestSchemaTransformer : IOpenApiOperationTransformer
{
    public async Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (context.Description.HttpMethod == "POST"
            && string.Equals(
                context.Description.RelativePath,
                "api/events",
                StringComparison.OrdinalIgnoreCase))
        {
            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = await context.GetOrCreateSchemaAsync(
                            typeof(CreateEventRequest), parameterDescription: null, cancellationToken),
                    },
                },
            };
        }
    }
}
