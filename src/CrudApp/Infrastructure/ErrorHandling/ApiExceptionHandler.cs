using CrudApp.Infrastructure.UtilityCode;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.WebUtilities;

namespace CrudApp.Infrastructure.ErrorHandling;

public class ApiExceptionHandler : IAsyncExceptionFilter
{
    public static bool IsExceptionDetailsInResponseEnabled { get; set; }

    private readonly ProblemDetailsFactory _problemDetailsFactory;
    private readonly ILogger<ApiExceptionHandler> _logger;

    public ApiExceptionHandler(
        ProblemDetailsFactory problemDetailsFactory,
        ILogger<ApiExceptionHandler> logger)
    {
        _problemDetailsFactory = problemDetailsFactory;
        _logger = logger;
    }

    public async Task OnExceptionAsync(ExceptionContext context)
    {
        var exception = context.Exception;
        if (context.HttpContext.Response.HasStarted)
        {
            _logger.Log(GetLogLevel(exception), exception, "Ignoring exception because response has started.");
            context.ExceptionHandled = true;
            return;
        }
        
        switch (exception)
        {
            case NotAuthenticatedException:
                _logger.Log(GetLogLevel(exception), exception, "Handling exception by issuing an authentication challenge.");
                await context.HttpContext.ChallengeAsync();
                break;
            case NotAuthorizedException:
                _logger.Log(GetLogLevel(exception), exception, "Handling exception by forbidding the request.");
                await context.HttpContext.ForbidAsync();
                break;
            default:
                _logger.Log(GetLogLevel(exception), exception, "Handling exception by returning problem details.");
                var problemDetails = exception switch
                {
                    ApiResponseException ex => CreateProblemDetails(context.HttpContext, ex),
                    ValidationException ex => CreateValidationProblemDetails(context.HttpContext, ex),
                    Exception ex => CreateInternalServerErrorProblemDetails(context.HttpContext)
                };
                IncludeExceptionDetails(problemDetails, exception);
                context.Result = new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
                break;
        }
        context.ExceptionHandled = true;
    }

    private static LogLevel GetLogLevel(Exception exception) => exception switch
    {
        NotAuthenticatedException => LogLevel.Debug,
        NotAuthorizedException => LogLevel.Debug,
        ApiResponseException ex => ex.HttpStatus < 500 ? LogLevel.Debug : LogLevel.Error,
        ValidationException => LogLevel.Debug,
        Exception => LogLevel.Error
    };

    private ProblemDetails CreateProblemDetails(HttpContext httpContext, ApiResponseException ex)
    {
        var problemDetails = _problemDetailsFactory.CreateProblemDetails(
            httpContext,
            statusCode: ex.HttpStatus,
            title: ReasonPhrases.GetReasonPhrase(ex.HttpStatus),
            detail: ex.GetMessagesIncludingData(e => e is ApiResponseException));
        return problemDetails;
    }
    

    private ProblemDetails CreateValidationProblemDetails(HttpContext httpContext, ValidationException ex)
    {
        var problemDetails = _problemDetailsFactory.CreateValidationProblemDetails(
            httpContext,
            ex.ModelState);
        return problemDetails;
    }

    private ProblemDetails CreateInternalServerErrorProblemDetails(HttpContext httpContext)
    {
        var problemDetails = _problemDetailsFactory.CreateProblemDetails(
            httpContext,
            statusCode: HttpStatus.InternalServerError,
            title: ReasonPhrases.GetReasonPhrase(HttpStatus.InternalServerError));
        return problemDetails;
    }

    private static void IncludeExceptionDetails(ProblemDetails problemDetails, Exception exception)
    {
        if (IsExceptionDetailsInResponseEnabled)
        {
            problemDetails.Detail = exception.GetMessagesIncludingData();
            problemDetails.Extensions.Add("exception", exception.ToString());
        }
    }
}