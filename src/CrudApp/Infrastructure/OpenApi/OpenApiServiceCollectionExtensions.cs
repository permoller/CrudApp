using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace CrudApp.Infrastructure.OpenApi;

public static class OpenApiServiceCollectionExtensions
{
    public static IServiceCollection AddCrudAppOpenApi(this IServiceCollection services)
    {
        // Add information about responses for actions.
        // Can be overwritten using ProducesResponseType-attribute and Produces-attribute on individual actions.
        services.AddTransient<IApplicationModelProvider>((_) => new ResponseMetadataProvider("application/json"));


        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(swaggerGenOptions =>
        {
            // We have nullable-reference-types enabled in the project.
            // So properties where the type is a reference type that is not marked as nullable,
            // should also not be marked as nullable in the OpenAPI document.
            swaggerGenOptions.SupportNonNullableReferenceTypes();

            // Make sure non-nullable properties are marked as required (meaning they must appear in the JSON).
            swaggerGenOptions.SchemaFilter<RequireNonNullablePropertiesSchemaFilter>();
        });

        return services;
    }
}
