using Microsoft.AspNetCore.Mvc;

namespace CrudApp.Infrastructure.ErrorHandling;

public static class ErrorHandlingServiceCollectionExtensions
{
    public static IServiceCollection AddCrudAppExceptionHandling(this IServiceCollection services)
    {
        // Convert exceptions to a problem details response https://tools.ietf.org/html/rfc7807
        services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add<ApiExceptionHandler>();
        });

        // Add additional information to the returned problem details response.
        // This is done here using configuration of the options
        // and not just added in the ProblemDetailsExceptionHandler,
        // to also make it work if problem details are created (using the factory)
        // in other places like directly in a controller using the Problem-method
        // or when the automatic model validation fails.
        services.Configure<ProblemDetailsOptions>(options =>
        {
            options.CustomizeProblemDetails = ProblemDetailsHelper.CustomizeProblemDetails;
        });
        return services;
    }

    
}
