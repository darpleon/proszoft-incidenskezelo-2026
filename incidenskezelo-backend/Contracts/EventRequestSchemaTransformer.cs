using incidenskezelo_backend.Configuration;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

namespace incidenskezelo_backend.Contracts;

public class EventRequestSchemaTransformer : IOpenApiOperationTransformer
{
    public async Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var options = context.ApplicationServices
            .GetRequiredService<IOptions<EventIngestOptions>>().Value;

        if (!options.DirectHttpEnabled)
        {
            return;
        }

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
