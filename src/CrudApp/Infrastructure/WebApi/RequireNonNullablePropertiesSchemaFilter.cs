using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CrudApp.Infrastructure.WebApi;

/// <summary>
/// Marks properties that may not be null as required (meaning the properties must exists in JSON) in the OpenAPI document.
/// This means if you generate TypeScript types from the OpenAPI document,
/// the properties should be generated so you don't have to check them for undefined everywhere.
/// And because they may also not be null you also do not have to check for null everywhere.
/// </summary>
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
