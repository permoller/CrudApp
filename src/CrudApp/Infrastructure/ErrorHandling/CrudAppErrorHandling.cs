using Microsoft.AspNetCore.Mvc;

namespace CrudApp.Infrastructure.ErrorHandling;

public static class CrudAppErrorHandling
{
    public static IServiceCollection AddCrudAppErrorHandling(this IServiceCollection services)
    {
        // Convert exceptions to a problem details response
        services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add<ApiExceptionHandler>();
        });

        services.PostConfigure<ApiBehaviorOptions>(o =>
        {
            // When the build-in model validation fails, generate a custom problem details response
            o.InvalidModelStateResponseFactory = ProblemDetailsHelper.InvalidModelStateResponseFactory;
        });

        // Add additional information to the returned problem details response.
        // This will be added when the problem details are created using the ProblemDetailsFactory.
        services.Configure<ProblemDetailsOptions>(options =>
        {
            options.CustomizeProblemDetails = ProblemDetailsHelper.CustomizeProblemDetails;
        });
        return services;
    }
}
