using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace CrudApp.Infrastructure.ErrorHandling;

public static class ProblemDetailsHelper
{
    /// <summary>
    /// Setting this to true may expose confidential information in the responses.
    /// It is usefull for debugging, but should not be used in production.
    /// </summary>
    public static bool IncludeExceptionInProblemDetails { get; set; }

    public static readonly string TypePrefix = "/error-types/";
    public static readonly string InstancePrefix = "/error-instances/";


    /// <summary>
    /// Called when <see cref="ProblemDetailsFactory"/> is used to create a <see cref="ProblemDetails"/> object.
    /// This method adds additional debug information to <see cref="ProblemDetails.Extensions"/>.
    /// </summary>
    public static void CustomizeProblemDetails(ProblemDetailsContext problemDetailsContext)
    {
        var problemDetails = problemDetailsContext.ProblemDetails;
        problemDetails.Extensions.TryAdd("serverTime", DateTimeOffset.Now);
        if (problemDetailsContext.HttpContext is not null)
        {
            problemDetails.Extensions.TryAdd("traceId", Activity.Current?.Id);
            problemDetails.Extensions.TryAdd("traceIdentifier", problemDetailsContext.HttpContext.TraceIdentifier);
            var identity = problemDetailsContext.HttpContext.User.Identities.FirstOrDefault(i => i.IsAuthenticated);
            if (identity is not null)
                problemDetails.Extensions.TryAdd("user", identity.Name);

            var request = problemDetailsContext.HttpContext?.Request;
            if (request is not null)
            {
                problemDetailsContext.ProblemDetails.Extensions.TryAdd("path", request.Path.Value);
                problemDetailsContext.ProblemDetails.Extensions.TryAdd("controller", request.RouteValues["controller"]?.ToString());
                problemDetailsContext.ProblemDetails.Extensions.TryAdd("action", request.RouteValues["action"]?.ToString());
            }
        }
        IncludeException(problemDetails, problemDetailsContext.Exception);
    }

    /// <summary>
    /// <see cref="EntityControllerBase{T}"/> is marked with <see cref="ApiControllerAttribute"/>
    /// which triggers automatic model validation before that action-method is called.
    /// If the validation fails, this method is called to get the response.
    /// This method converts the <see cref="ModelStateDictionary"/> with the errors to a <see cref="Error.ValidationFailed"/>
    /// which is then mapped to a <see cref="ProblemDetails"/> and returned in an <see cref="ObjectResult"/>.
    /// </summary>
    public static IActionResult InvalidModelStateResponseFactory(ActionContext context)
    {
        // Use the functionality in ValidationProblemDetails to convert ModelStateDictionary to an errors dictionary.
        var validationErrors = new ValidationProblemDetails(context.ModelState).Errors;
        var errors = validationErrors as Dictionary<string, string[]> ?? new Dictionary<string, string[]>(validationErrors);

        var error = new Error.ValidationFailed(errors);
        var problemDetails = context.HttpContext.MapErrorToProblemDetails(error);
        return problemDetails.ToObjectResult();
    }

    public static ObjectResult ToObjectResult(this ProblemDetails problemDetails) =>
        new(problemDetails) { StatusCode = problemDetails.Status };

    public static ProblemDetails MapErrorToProblemDetails(this HttpContext httpContext, Error error)
    {
        var problemDetailsFactory = httpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        var problemDetails = problemDetailsFactory.CreateProblemDetails(
            httpContext,
            statusCode: error.HttpStatucCode,
            type: TypePrefix + error.GetType().Name,
            instance: InstancePrefix + error.Instance,
            title: Regex.Replace(error.GetType().Name, "[a-z][A-Z]", m => m.Value[0] + " " + char.ToLower(m.Value[1])) + ".", // ClassName -> Class name.
            detail: null);

        // The trace id also contains the span id from when the problem details was created.
        // But the trace id on the error is captured when the error occoured which might contain a different span id.
        // So we overwrite the trace id if we got one.
        if (error.TraceId is not null)
            problemDetails.Extensions["traceId"] = error.TraceId;

        if (error.Data.Count > 0)
            problemDetails.Extensions["data"] = error.Data;

        if (error.Errors.Count > 0)
            problemDetails.Extensions["errors"] = error.Errors;
        var jsonObj = new JsonObject();
        jsonObj.Add("testProp", JsonValue.Create(Guid.NewGuid()));
        problemDetails.Extensions["xxx"] = jsonObj;
        IncludeException(problemDetails, error.Exception);

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
        IncludeException(problemDetails, exception);
        return problemDetails;


        static ProblemDetails CreateProblemDetails(HttpContext httpContext, ApiResponseException ex)
        {
            var problemDetailsFactory = httpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
            var problemDetails = problemDetailsFactory.CreateProblemDetails(
                httpContext,
                statusCode: ex.HttpStatus,
                title: ReasonPhrases.GetReasonPhrase(ex.HttpStatus),
                detail: ex.GetMessagesIncludingData(e => e is ApiResponseException));
            return problemDetails;
        }

        static ValidationProblemDetails CreateValidationProblemDetails(HttpContext httpContext, ValidationException ex)
        {
            var problemDetailsFactory = httpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
            var problemDetails = problemDetailsFactory.CreateValidationProblemDetails(
                httpContext,
                ex.ModelState);
            return problemDetails;
        }

        static ProblemDetails CreateInternalServerErrorProblemDetails(HttpContext httpContext)
        {
            var problemDetailsFactory = httpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
            var problemDetails = problemDetailsFactory.CreateProblemDetails(
                httpContext,
                statusCode: HttpStatus.InternalServerError,
                title: ReasonPhrases.GetReasonPhrase(HttpStatus.InternalServerError));
            return problemDetails;
        }
    }

    private static void IncludeException(ProblemDetails problemDetails, Exception? exception)
    {
        if (IncludeExceptionInProblemDetails && exception is not null)
        {
            problemDetails.Extensions.Add("exceptionMessage", exception.GetMessagesIncludingData());
            problemDetails.Extensions.Add("exception", exception.ToString());
        }
    }

}
