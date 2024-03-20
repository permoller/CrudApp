using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace CrudApp.Infrastructure.WebApi;

/// <summary>
/// When <see cref="Result{T}"/> is returned from a controller action,
/// this filter extracts and returns the inner value of type T instead.
/// If the <see cref="Result{T}"/> contains an <see cref="Error"/> instead of a value,
/// the <see cref="Error"/> is transformed to a <see cref="ProblemDetails"/> and returned.
/// </summary>
public sealed class UnwrapResultActionFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if ((context.Result as ObjectResult)?.Value is Primitives.IResult result)
        {
            if (result.TryGetValue(out var value))
            {
                if (value is null || value is Nothing)
                    context.Result = new StatusCodeResult(HttpStatus.NoContent);
                else
                    context.Result = new ObjectResult(value);
            }
            else if (result.TryGetError(out var error))
            {
                var problemDetails = context.HttpContext.MapErrorToProblemDetails(error);
                context.Result = new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
            }
        }
    }

}

