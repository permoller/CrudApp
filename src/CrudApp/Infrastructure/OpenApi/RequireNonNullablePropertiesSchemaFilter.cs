using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CrudApp.Infrastructure.OpenApi;

public class RequireNonNullablePropertiesSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        foreach (var p in schema.Properties)
        {
            if (!p.Value.Nullable && !schema.Required.Contains(p.Key))
                schema.Required.Add(p.Key);
        }
    }
}
