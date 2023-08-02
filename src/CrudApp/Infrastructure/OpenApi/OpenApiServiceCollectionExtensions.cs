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
            // Maintain inheritance information in the OpenAPI document.
            swaggerGenOptions.UseAllOfForInheritance();

            // Support marking reference-properties (properties that contain an object) as nullable in the OpenAPI document.
            swaggerGenOptions.UseAllOfToExtendReferenceSchemas();

            // We have nullable reference types enabled in the project.
            // So properties where the type is a reference type that is not marked as nullable,
            // should also not be marked as nullable in the OpenAPI document.
            swaggerGenOptions.SupportNonNullableReferenceTypes();
            
            // Make sure non-nullable properties are marked as required (meaning they must appear in the JSON),
            // to require less checks for undefined in TypeScript when coding agains generated types.
            swaggerGenOptions.SchemaFilter<RequireNonNullablePropertiesSchemaFilter>();
        });

        return services;
    }
}
