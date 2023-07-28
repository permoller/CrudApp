using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.WebUtilities;

namespace CrudApp.Infrastructure.ErrorHandling;

/// <summary>
/// Return ProblemDetails response on exceptions.
/// </summary>
public class ProblemDetailsExceptionHandler : IExceptionFilter
{
    private readonly ProblemDetailsFactory _problemDetailsFactory;
    private readonly IHostEnvironment _hostEnvironment;

    public ProblemDetailsExceptionHandler(ProblemDetailsFactory problemDetailsFactory, IHostEnvironment hostEnvironment)
    {
        _problemDetailsFactory = problemDetailsFactory;
        _hostEnvironment = hostEnvironment;
    }
    public void OnException(ExceptionContext context)
    {
        var problemDetails = context.Exception switch
        {
            ValidationException ex => CreateValidationProblemDetails(context.HttpContext, ex),
            ApiResponseException ex => CreateProblemDetails(context.HttpContext, ex),
            Exception ex => CreateInternalServerErrorProblemDetails(context.HttpContext, ex)
        };

        context.Result = new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        context.ExceptionHandled = true;
    }

    private ProblemDetails CreateProblemDetails(HttpContext httpContext, ApiResponseException ex)
    {
        var problemDetails = _problemDetailsFactory.CreateProblemDetails(
            httpContext,
            statusCode: (int)ex.HttpStatus,
            title: ReasonPhrases.GetReasonPhrase((int)ex.HttpStatus),
            detail: ex.Message);

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