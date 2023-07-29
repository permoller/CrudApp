using Microsoft.AspNetCore.Mvc;

namespace CrudApp.Infrastructure.ErrorHandling;

public static class ProblemDetailsServiceCollectionExtensions
{
    public static IServiceCollection AddProblemDetailsExceptionHandler(this IServiceCollection services)
    {
        // Convert exceptions to a problem details response https://tools.ietf.org/html/rfc7807
        services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add<ProblemDetailsExceptionHandler>();
        });

        // Add additional information to the returned problem details response.
        // This is done here using configuration of the options
        // and not just added in the ProblemDetailsExceptionHandler,
        // to also make it work if problem details are created (using the factory)
        // in other places like directly in a controller using the Problem-method
        // or when the automatic model validation fails.
        services.Configure<ProblemDetailsOptions>(options =>
        {
            options.CustomizeProblemDetails = CustomizeProblemDetails;
        });
        return services;
    }

    private static void CustomizeProblemDetails(ProblemDetailsContext problemDetailsContext)
    {
        problemDetailsContext.ProblemDetails.Extensions.Add("serverTimeUtc", DateTimeOffset.UtcNow);

        var request = problemDetailsContext.HttpContext?.Request;
        if (request is not null)
        {
            var path = request.Path.Value;
            var controller = request.RouteValues["controller"]?.ToString();
            var action = request.RouteValues["action"]?.ToString();

            problemDetailsContext.ProblemDetails.Extensions.Add("path", path);
            problemDetailsContext.ProblemDetails.Extensions.Add("controller", controller);
            problemDetailsContext.ProblemDetails.Extensions.Add("action", action);
        }
    }
}
