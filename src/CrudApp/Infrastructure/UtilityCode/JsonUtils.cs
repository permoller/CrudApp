using System.Text.Encodings.Web;
using System.Text.Json;

namespace CrudApp.Infrastructure.UtilityCode;

public static class JsonUtils
{
    public static IServiceCollection AddCrudAppJsonOptions(this  IServiceCollection services)
    {
        // The settings used by default by System.Text.Json.JsonSerializer can not be configured.
        // And they are not compatible with the those used by the API by default.
        // We need to provide the options explicitly when using JsonSerializer directly (or indirectly like the JSON extension methods on HttpClient and HttpResponseMessage).
        // Alternativly we could configure the options used in the API to be compatible, but that means we would be locked in to those options.

        // Configure JSON options for minimal API (HttpRequestJsonExtensions and HttpResponseJsonExtensions)
        services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options => ConfigureApiJsonSerializerOptions(options.SerializerOptions));

        // Configure JSON options for MVC-Controllers (SystemTextJsonInputFormatter, SystemTextJsonOutputFormatter)
        services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options => ConfigureApiJsonSerializerOptions(options.JsonSerializerOptions));

        return services;
    }

    /// <summary>
    /// These are the settings used for JSON in the web services API.
    /// </summary>
    public static JsonSerializerOptions ApiJsonSerializerOptions { get; } = ConfigureApiJsonSerializerOptions();

    /// <summary>
    /// These are the settings used when representing objects as JSON-strings in the database.
    /// </summary>
    public static JsonSerializerOptions DbJsonSerializerOptions { get; } = JsonSerializerOptions.Default;



    private static JsonSerializerOptions ConfigureApiJsonSerializerOptions(JsonSerializerOptions? options = null)
    {
        // Wee align the options made by Microsoft.AspNetCore.Http.Json.JsonOptions and Microsoft.AspNetCore.Mvc.JsonOptions
        // to make the JSON format of the API uniform.

        // Both create the options using JsonSerializerDefaults.Web
        if (options is null)
            options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        // Microsoft.AspNetCore.Http.Json.JsonOptions sets the encoder
        options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping; // the default escapes the json to make it safe to embed in HTML... we do not need that in the API

        // Microsoft.AspNetCore.Mvc.JsonOptions sets the max depth
        options.MaxDepth = 32; // Magic number comes from Microsoft.AspNetCore.Mvc.MvcOptions.DefaultMaxModelBindingRecursionDepth (it is internal)


        // Leave property names as-is when serializing.
        // It does not make sense to convert the names to camelCase, and then having to convert back when deserializing to C#/Typescript classes.
        options.PropertyNamingPolicy = null;
        
        return options;
    }

    
}
