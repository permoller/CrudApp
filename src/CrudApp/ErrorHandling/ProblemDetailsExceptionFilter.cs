﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.WebUtilities;
using static Microsoft.AspNetCore.Http.StatusCodes;


namespace CrudApp.ErrorHandling;

/// <summary>
/// Return ProblemDetails response on exceptions.
/// </summary>
public class ProblemDetailsExceptionFilter : IExceptionFilter
{
    private readonly ProblemDetailsFactory _problemDetailsFactory;
    private readonly IHostEnvironment _hostEnvironment;

    public ProblemDetailsExceptionFilter(ProblemDetailsFactory problemDetailsFactory, IHostEnvironment hostEnvironment)
    {
        _problemDetailsFactory = problemDetailsFactory;
        _hostEnvironment = hostEnvironment;
    }
    public void OnException(ExceptionContext context)
    {
        var problemDetails = context.Exception switch
        {
            ValidationException ex => CreateValidationProblemDetails(context.HttpContext, ex),
            ProblemDetailsException ex => CreateProblemDetails(context.HttpContext, ex),
            Exception ex => CreateInternalServerErrorProblemDetails(context.HttpContext, ex)
        };

        // TODO: Add more information like controller, action and url. A timestamp may also be missing.

        context.Result = new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
        context.ExceptionHandled = true;
    }

    private ProblemDetails CreateProblemDetails(HttpContext httpContext, ProblemDetailsException ex)
    {
        var problemDetails = _problemDetailsFactory.CreateProblemDetails(
            httpContext,
            statusCode: (int)ex.StatusCode,
            title: ReasonPhrases.GetReasonPhrase((int)ex.StatusCode),
            type: $"/errors/{GetType().Name}",
            detail: ex.Message);

        if (_hostEnvironment.IsDevelopment() && ex.InnerException is not null)
        {
            problemDetails.Extensions.Add("exception", ex.InnerException.ToString());
        }
        return problemDetails;
    }

    private ProblemDetails CreateValidationProblemDetails(HttpContext httpContext, ValidationException ex) =>
        _problemDetailsFactory.CreateValidationProblemDetails(
            httpContext,
            ex.ModelState,
            type: $"/errors/{GetType().Name}",
            detail: ex.Message);

    private ProblemDetails CreateInternalServerErrorProblemDetails(HttpContext httpContext, Exception ex)
    {
        var problemDetails = _problemDetailsFactory.CreateProblemDetails(
            httpContext,
            Status500InternalServerError,
            "Internal server error",
            $"errors/InternalServerError");
        
        if (_hostEnvironment.IsDevelopment())
        {
            problemDetails.Extensions.Add("exception", ex.ToString());
        }

        return problemDetails;
    }
}