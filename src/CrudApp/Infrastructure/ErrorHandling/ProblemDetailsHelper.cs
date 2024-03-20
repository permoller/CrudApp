using CrudApp.Infrastructure.Primitives;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.WebUtilities;

namespace CrudApp.Infrastructure.ErrorHandling;

public static class ProblemDetailsHelper
{
    public static bool IncludeExceptionInProblemDetails { get; set; }

    public static void CustomizeProblemDetails(ProblemDetailsContext problemDetailsContext)
    {
        problemDetailsContext.ProblemDetails.Extensions.TryAdd("serverTime", DateTimeOffset.Now);
        problemDetailsContext.ProblemDetails.Extensions.TryAdd("traceIdentifier", problemDetailsContext.HttpContext?.TraceIdentifier);

        var request = problemDetailsContext.HttpContext?.Request;
        if (request is not null)
        {
            var path = request.Path.Value;
            var controller = request.RouteValues["controller"]?.ToString();
            var action = request.RouteValues["action"]?.ToString();

            problemDetailsContext.ProblemDetails.Extensions.TryAdd("path", path);
            problemDetailsContext.ProblemDetails.Extensions.TryAdd("controller", controller);
            problemDetailsContext.ProblemDetails.Extensions.TryAdd("action", action);
        }
    }

    public static ProblemDetails MapErrorToProblemDetails(this HttpContext httpContext, Error error)
    {
        var problemDetailsFactory = httpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        var problemDetails = problemDetailsFactory.CreateProblemDetails(
            httpContext,
            statusCode: (int)error.ErrorStatus,
            type: string.IsNullOrEmpty(error.Type) ? error.ErrorStatus.ToString() : error.Type,
            title: string.IsNullOrEmpty(error.Title) ? ReasonPhrases.GetReasonPhrase((int)error.ErrorStatus) : error.Title,
            detail: error.Detail,
            instance: error.Instance);

        IncludeExceptionDetails(problemDetails, error.Exception);

        foreach (var kvp in error.Data)
            problemDetails.Extensions[kvp.Key] = kvp.Value;

        return problemDetails;
    }

    public static ProblemDetails MapExceptionToProblemDetails(this HttpContext httpContext, Exception exception)
    {
        var problemDetails = exception switch
        {
            ApiResponseException ex => CreateProblemDetails(httpContext, ex),
            ValidationException ex => CreateValidationProblemDetails(httpContext, ex),
            Exception _ => CreateInternalServerErrorProblemDetails(httpContext)
        };
        IncludeExceptionDetails(problemDetails, exception);
        return problemDetails;
    }

    
    private static ProblemDetails CreateProblemDetails(HttpContext httpContext, ApiResponseException ex)
    {
        var problemDetailsFactory = httpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        var problemDetails = problemDetailsFactory.CreateProblemDetails(
            httpContext,
            statusCode: ex.HttpStatus,
            title: ReasonPhrases.GetReasonPhrase(ex.HttpStatus),
            detail: ex.GetMessagesIncludingData(e => e is ApiResponseException));
        return problemDetails;
    }


    private static ValidationProblemDetails CreateValidationProblemDetails(HttpContext httpContext, ValidationException ex)
    {
        var problemDetailsFactory = httpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        var problemDetails = problemDetailsFactory.CreateValidationProblemDetails(
            httpContext,
            ex.ModelState);
        return problemDetails;
    }

    private static ProblemDetails CreateInternalServerErrorProblemDetails(HttpContext httpContext)
    {
        var problemDetailsFactory = httpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        var problemDetails = problemDetailsFactory.CreateProblemDetails(
            httpContext,
            statusCode: HttpStatus.InternalServerError,
            title: ReasonPhrases.GetReasonPhrase(HttpStatus.InternalServerError));
        return problemDetails;
    }

    private static void IncludeExceptionDetails(ProblemDetails problemDetails, Exception? exception)
    {
        if (IncludeExceptionInProblemDetails && exception is not null)
        {
            problemDetails.Extensions.Add("exceptionMessage", exception.GetMessagesIncludingData());
            problemDetails.Extensions.Add("exceptionToString", exception.ToString());
        }
    }
}
