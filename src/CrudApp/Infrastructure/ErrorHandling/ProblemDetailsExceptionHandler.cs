using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.WebUtilities;

namespace CrudApp.Infrastructure.ErrorHandling;

/// <summary>
/// Return ProblemDetails response on exceptions.
/// </summary>
public class ProblemDetailsExceptionHandler : IAsyncExceptionFilter
{
    private readonly ProblemDetailsFactory _problemDetailsFactory;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<ProblemDetailsExceptionHandler> _logger;

    public ProblemDetailsExceptionHandler(
        ProblemDetailsFactory problemDetailsFactory,
        IHostEnvironment hostEnvironment,
        ILogger<ProblemDetailsExceptionHandler> logger)
    {
        _problemDetailsFactory = problemDetailsFactory;
        _hostEnvironment = hostEnvironment;
        _logger = logger;
    }

    public async Task OnExceptionAsync(ExceptionContext context)
    {
        
        var problemDetails = context.Exception switch
        {
            ApiResponseException ex => CreateProblemDetails(context.HttpContext, ex),
            ValidationException ex => CreateValidationProblemDetails(context.HttpContext, ex),
            Exception ex => CreateInternalServerErrorProblemDetails(context.HttpContext, ex)
        };

        if(problemDetails.Status < 500)
            _logger.LogInformation(context.Exception, "Exception");
        else
            _logger.LogError(context.Exception, "Exception");
        
        var hasResponseStarted = context.HttpContext.Response.HasStarted;
        _logger.LogDebug("hasResponseStarted:{hasResponseStarted}", hasResponseStarted);
        if (hasResponseStarted)
            return;
        

        if (problemDetails.Status == 401)
            await context.HttpContext.ChallengeAsync();
        else
            context.Result = new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        context.ExceptionHandled = true;
    }

    private ProblemDetails CreateProblemDetails(HttpContext httpContext, ApiResponseException ex)
    {
        var problemDetails = _problemDetailsFactory.CreateProblemDetails(
            httpContext,
            statusCode: (int)ex.HttpStatus,
            title: ReasonPhrases.GetReasonPhrase((int)ex.HttpStatus),
            detail: ex.HasMessage ? ex.Message : null);
        
        if (_hostEnvironment.IsDevelopment())
        {
            problemDetails.Extensions.Add("exception", ex.ToString());
        }
        return problemDetails;
    }

    private ProblemDetails CreateValidationProblemDetails(HttpContext httpContext, ValidationException ex) =>
        _problemDetailsFactory.CreateValidationProblemDetails(
            httpContext,
            ex.ModelState);

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