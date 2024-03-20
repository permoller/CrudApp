using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CrudApp.Infrastructure.ErrorHandling;

public class ApiExceptionHandler : IAsyncExceptionFilter
{
    private readonly ILogger<ApiExceptionHandler> _logger;

    public ApiExceptionHandler(ILogger<ApiExceptionHandler> logger)
    {
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
                var problemDetails = context.HttpContext.MapExceptionToProblemDetails(exception);
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
}