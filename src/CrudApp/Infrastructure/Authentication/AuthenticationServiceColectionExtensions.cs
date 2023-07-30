using Microsoft.AspNetCore.Authentication;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;

namespace CrudApp.Infrastructure.Authentication;

public static class AuthenticationServiceColectionExtensions
{

    public const string SchemeNameInConfiguration = nameof(UserIdAuthenticationHandler);

    public static IServiceCollection AddCrudAppAuthentication(this IServiceCollection services)
    {
        services.AddTransient<UserIdAuthenticationHandler>();
        
        services.Configure<AuthenticationOptions>(options =>
        {
            options.AddScheme(SchemeNameInConfiguration, scheme =>
            {
                scheme.HandlerType = typeof(UserIdAuthenticationHandler);
                scheme.DisplayName = SchemeNameInConfiguration;
            });
        });

        services.AddAuthentication(defaultScheme: SchemeNameInConfiguration);

        services.ConfigureSwaggerGen(options =>
        {
            var scheme = new OpenApiSecurityScheme()
            {
                Description = $"Use a value like: {UserIdAuthenticationHandler.HttpAuthenticationScheme} 123",
                Type = SecuritySchemeType.ApiKey, // using the type ApiKey allows us to say that we use the Authorization HTTP header without using one of the normal schemes
                In = ParameterLocation.Header,
                Name = HeaderNames.Authorization,
            };
            var schemeId = "UserId authentication";
            options.AddSecurityDefinition(schemeId, scheme);

            var requirement = new OpenApiSecurityRequirement();

            var schemeReference = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = schemeId
                }
            };
            requirement.Add(schemeReference, new List<string>());
            options.AddSecurityRequirement(requirement);
        });
        return services;
    }
}
