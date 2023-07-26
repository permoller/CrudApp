using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace CrudApp.Infrastructure.OpenApi;

/// <summary>
/// Used to set the default success response type on actions without explicitly defined response types.
/// </summary>
public class ResponseMetadataProvider : IApplicationModelProvider
{
    private static readonly Type[] _genericTaskTypes = new[] { typeof(Task<>), typeof(ValueTask<>) };
    private readonly string _contentType;

    public int Order => 0;

    public ResponseMetadataProvider(string contentType)
    {
        _contentType = contentType;
    }

    public void OnProvidersExecuted(ApplicationModelProviderContext context)
    {
    }

    public void OnProvidersExecuting(ApplicationModelProviderContext context)
    {
        foreach (var controller in context.Result.Controllers)
        {
            foreach (var action in controller.Actions)
            {
                EnsureSuccessResponseDefined(action);
                EnsureErrorResponsesDefined(action);
                EnsureContentTypeDefined(action, _contentType);
            }
        }
    }

    private static void EnsureSuccessResponseDefined(ActionModel action)
    {
        var isSuccessResponseDefined = GetAttributes(action)
            .OfType<IApiResponseMetadataProvider>()
            .Any(x => x.StatusCode >= 200 && x.StatusCode < 300);

        if (!isSuccessResponseDefined)
        {
            var returnType = action.ActionMethod.ReturnType;

            if (returnType.IsGenericType && _genericTaskTypes.Contains(returnType.GetGenericTypeDefinition()))
                returnType = returnType.GetGenericArguments()[0];

            if (returnType.IsAssignableTo(typeof(IActionResult)) || returnType.IsAssignableTo(typeof(IConvertToActionResult)))
                throw new NotSupportedException($"Actions that return {nameof(IActionResult)} or {nameof(IConvertToActionResult)} should explicitly specify the appropate success response types using {nameof(ProducesResponseTypeAttribute)}. This action does not: {action.DisplayName}.");

            action.Filters.Add(new ProducesResponseTypeAttribute(returnType, (int)HttpStatus.Ok));
        }
    }

    private static void EnsureErrorResponsesDefined(ActionModel action)
    {
        var existingStatusCodes = GetAttributes(action)
            .OfType<IApiResponseMetadataProvider>()
            .Select(x => x.StatusCode)
            .ToList();
        foreach (var statusCode in Enum.GetValues<HttpStatus>().Where(s => (int)s >= 400))
        {
            if (!existingStatusCodes.Contains((int)statusCode))
            {
                action.Filters.Add(new ProducesResponseTypeAttribute(typeof(ProblemDetails), (int)statusCode));
            }
        }
    }

    private static void EnsureContentTypeDefined(ActionModel action, string contentType)
    {
        var isContentTypeDefined = GetAttributes(action)
            .OfType<ProducesAttribute>()
            .Any();
        if (!isContentTypeDefined)
        {
            action.Filters.Add(new ProducesAttribute(contentType));
        }
    }

    private static IEnumerable<object> GetAttributes(ActionModel action)
    {
        return action.ActionMethod.GetCustomAttributes(true)
            .Concat(action.Controller.ControllerType.GetCustomAttributes(true));
    }
}