using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace CrudApp.Infrastructure.WebApi;

public partial class CrudAppApplicationModelProvider: IApplicationModelProvider
{
    private static readonly Type[] _voidReturnTypes = new[] { typeof(void), typeof(Task), typeof(ValueTask), typeof(Nothing) };

    public int Order => 0;

    public void OnProvidersExecuting(ApplicationModelProviderContext context)
    {
        foreach (var controller in context.Result.Controllers)
        {
            foreach (var action in controller.Actions)
            {
                var returnType = ApplyResultFilterAndGetUnwrappedReturnType(action);
                EnsureSuccessResponseDefined(action, returnType);
                EnsureErrorResponsesDefined(action);
            }
        }
    }

    public void OnProvidersExecuted(ApplicationModelProviderContext context)
    {
    }

    private static Type ApplyResultFilterAndGetUnwrappedReturnType(ActionModel action)
    {
        var returnType = action.ActionMethod.ReturnType;
        if (returnType.TryGetGenericArgumentsForGenericTypeDefinition(typeof(Task<>), out var taskArgs))
            returnType = taskArgs[0];
        if (returnType.TryGetGenericArgumentsForGenericTypeDefinition(typeof(ValueTask<>), out var valueTaskArgs))
            returnType = valueTaskArgs[0];
        if (returnType.TryGetGenericArgumentsForGenericTypeDefinition(typeof(Result<>), out var resultTypeArgs))
        {
            action.Filters.Add(new UnwrapResultActionFilter());
            returnType = resultTypeArgs[0];
        }
        if (_voidReturnTypes.Contains(returnType))
            returnType = typeof(void);
        return returnType;
    }

    private static void EnsureSuccessResponseDefined(ActionModel action, Type returnType)
    {
        var isSuccessResponseDefined = GetAttributes(action)
            .OfType<IApiResponseMetadataProvider>()
            .Any(x => x.StatusCode >= 200 && x.StatusCode < 300);

        if (!isSuccessResponseDefined)
        {
            if (returnType.IsAssignableTo(typeof(IActionResult)) || returnType.IsAssignableTo(typeof(IConvertToActionResult)))
                throw new NotSupportedException($"Actions that return {nameof(IActionResult)} or {nameof(IConvertToActionResult)} should explicitly specify the appropate success response types using {nameof(ProducesResponseTypeAttribute)}. This action does not: {action.DisplayName}.");

            if (_voidReturnTypes.Contains(returnType))
            {
                action.Filters.Add(new ReturnNoContentStatusCode());
                action.Filters.Add(new ApiResponseMetadataProvider(typeof(void), HttpStatus.NoContent));
            }
            else
            {
                action.Filters.Add(new ApiResponseMetadataProvider(returnType, HttpStatus.Ok));
                if (returnType.MayTypeBeNull() == true)
                    action.Filters.Add(new ApiResponseMetadataProvider(typeof(void), HttpStatus.NoContent));
            }
        }
    }

    private static void EnsureErrorResponsesDefined(ActionModel action)
    {
        var existingStatusCodes = GetAttributes(action)
            .OfType<IApiResponseMetadataProvider>()
            .Select(x => x.StatusCode)
            .ToList();
        foreach (var statusCode in HttpStatus.UsedReturnStatusCodes.Where(s => s >= 400))
        {
            if (!existingStatusCodes.Contains(statusCode))
            {
                action.Filters.Add(new ApiResponseMetadataProvider(typeof(ProblemDetails), statusCode));
            }
        }
    }

    private static IEnumerable<object> GetAttributes(ActionModel action)
    {
        return action.ActionMethod.GetCustomAttributes(inherit: true)
            .Concat(action.Controller.ControllerType.GetCustomAttributes(inherit: true));
    }


    private sealed class ReturnNoContentStatusCode : IAsyncResultFilter
    {
        public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            // Default to returning 204 no content instead of 200
            context.HttpContext.Response.StatusCode = HttpStatus.NoContent;
            return next();
        }
    }

    private sealed class ApiResponseMetadataProvider : IApiResponseMetadataProvider
    {
        public ApiResponseMetadataProvider(Type? type, int statusCode)
        {
            Type = type;
            StatusCode = statusCode;
        }

        public Type? Type { get; }
        public int StatusCode { get; }

        public void SetContentTypes(MediaTypeCollection contentTypes)
        {
            contentTypes.Clear();
            contentTypes.Add("application/json");
        }
    }
}
