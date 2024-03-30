using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace CrudApp.Infrastructure.WebApi;

/// <summary>
/// Simplifies the API results by mapping some types.
/// <see cref="Result{T}"/> is mapped to its inner <see cref="Error"/> if there is an error or to its inner value of type T.
/// <see cref="Maybe{T}"/> is mapped to its inner value if T is a reference type or to a <see cref="Nullable{T}"/> if T is a value type.
/// <see cref="Error"/> is mapped to <see cref="ProblemDetails"/>.
/// <see cref="Nothing"/>, <see cref="Task"/>, <see cref="ValueTask"/> and null-values are mapped to a <see cref="NoContentResult"/>.
/// </summary>
public sealed class ReturnValueActionFilter() : IActionFilter
{
    private static readonly Type?[] _voidReturnTypes = [null, typeof(void), typeof(Task), typeof(ValueTask), typeof(Nothing)];

    public void OnActionExecuting(ActionExecutingContext context)
    {
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Result is ObjectResult objectResult)
        {
            var value = MapValue(objectResult.Value);

            if (value is null)
                context.Result = new NoContentResult();
            else if(value is Error error)
                context.Result = context.HttpContext.MapErrorToProblemDetails(error).ToObjectResult();
            else if (value != objectResult.Value)
            {
                objectResult.DeclaredType = value.GetType();
                objectResult.Value = value;
            }
        }
        else if(context.Result is EmptyResult)
        {
            // return status 204 NoContent instead of 200 Ok
            context.Result = new NoContentResult();
        }
    }

    private static object? MapValue(object? value)
    {
        while (true)
        {
            // Tasks are already unwrapped so we only need to unwrap our own primitive types
            if (value is IInfrastructureResult result)
                value = result.GetValueOrError();
            else if (value is IInfrastructureMaybe maybe)
                value = maybe.GetValueOrNull();
            else
                break;
        }

        if (value is not null && _voidReturnTypes.Contains(value.GetType()))
            value = null;

        return value;
    }

    public static Type MapType(ActionModel action)
    {
        var returnType = action.ActionMethod.ReturnType;
        while (true)
        {
            if (returnType.TryGetGenericArgumentsForGenericTypeDefinition(typeof(Task<>), out var taskArgs))
                returnType = taskArgs[0];
            else if (returnType.TryGetGenericArgumentsForGenericTypeDefinition(typeof(ValueTask<>), out var valueTaskArgs))
                returnType = valueTaskArgs[0];
            else if (returnType.TryGetGenericArgumentsForGenericTypeDefinition(typeof(ActionResult<>), out var actionResultArgs))
                returnType = actionResultArgs[0];
            else if (returnType.TryGetGenericArgumentsForGenericTypeDefinition(typeof(Result<>), out var resultTypeArgs))
                returnType = resultTypeArgs[0];
            else if (returnType.TryGetGenericArgumentsForGenericTypeDefinition(typeof(Maybe<>), out var maybeTypeArgs))
                returnType = ToNullable(maybeTypeArgs[0]);
            else
                break;
        }

        if (_voidReturnTypes.Contains(returnType))
            returnType = typeof(void);

        return returnType;
    }

    private static Type ToNullable(Type type) =>
        type.IsValueType ? typeof(Nullable<>).MakeGenericType(type) : type;
}

