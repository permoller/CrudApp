using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.WebUtilities;

namespace CrudApp.Infrastructure.ErrorHandling;

public class ApiExceptionHandler : IAsyncExceptionFilter
{
    private readonly ProblemDetailsFactory _problemDetailsFactory;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<ApiExceptionHandler> _logger;

    public ApiExceptionHandler(
        ProblemDetailsFactory problemDetailsFactory,
        IHostEnvironment hostEnvironment,
        ILogger<ApiExceptionHandler> logger)
    {
        _problemDetailsFactory = problemDetailsFactory;
        _hostEnvironment = hostEnvironment;
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
        
        switch (context.Exception)
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
                    Exception ex => CreateInternalServerErrorProblemDetails(context.HttpContext, ex)
                };
                context.Result = new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
                break;
        }
        context.ExceptionHandled = true;
    }

    private static LogLevel GetLogLevel(Exception exception) => exception switch
    {
        NotAuthenticatedException => LogLevel.Debug,
        NotAuthorizedException => LogLevel.Debug,
        ApiResponseException ex => (int)ex.HttpStatus < 500 ? LogLevel.Debug : LogLevel.Error,
        ValidationException => LogLevel.Debug,
        Exception => LogLevel.Error
    };

    private ProblemDetails CreateProblemDetails(HttpContext httpContext, ApiResponseException ex)
    {
        var problemDetails = _problemDetailsFactory.CreateProblemDetails(
            httpContext,
            statusCode: (int)ex.HttpStatus,
            title: ReasonPhrases.GetReasonPhrase((int)ex.HttpStatus),
            detail: ex.HasMessage ? ex.Message : null);
        
        if (_hostEnvironment.IsDevelopment() && ex.InnerException is not null)
        {
            problemDetails.Extensions.Add("exception", ex.InnerException.ToString());
        }
        return problemDetails;
    }

    private ProblemDetails CreateValidationProblemDetails(HttpContext httpContext, ValidationException ex)
    {
        var problemDetails = _problemDetailsFactory.CreateValidationProblemDetails(
            httpContext,
            ex.ModelState);
        return problemDetails;
    }

    private ProblemDetails CreateInternalServerErrorProblemDetails(HttpContext httpContext, Exception ex)
    {
        var problemDetails = _problemDetailsFactory.CreateProblemDetails(
            httpContext,
            statusCode: (int)HttpStatus.InternalServerError,
            title: ReasonPhrases.GetReasonPhrase((int)HttpStatus.InternalServerError));

        if (_hostEnvironment.IsDevelopment())
        {
            problemDetails.Extensions.Add("exception", ex.ToString());
        }

        return problemDetails;
    }
}