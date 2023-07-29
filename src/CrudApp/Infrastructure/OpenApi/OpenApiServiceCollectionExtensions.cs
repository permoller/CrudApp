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
            // Make sure non-nullable reference types are not marked as nullable.
            swaggerGenOptions.SupportNonNullableReferenceTypes();

            // Make sure all non-nullable properties are marked as required.
            swaggerGenOptions.SchemaFilter<RequireNonNullablePropertiesSchemaFilter>();
        });

        return services;
    }
}
