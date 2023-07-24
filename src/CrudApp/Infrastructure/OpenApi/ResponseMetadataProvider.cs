using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

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
                if (!IsSuccessResponseDefined(action))
                {
                    var returnType = action.ActionMethod.ReturnType;

                    if (returnType.IsGenericType && _genericTaskTypes.Contains(returnType.GetGenericTypeDefinition()))
                        returnType = returnType.GetGenericArguments()[0];

                    if (returnType.IsAssignableTo(typeof(IActionResult)) || returnType.IsAssignableTo(typeof(IConvertToActionResult)))
                        throw new NotSupportedException($"Actions that return {nameof(IActionResult)} or {nameof(IConvertToActionResult)} should explicitly specify the appropate success response types using {nameof(ProducesResponseTypeAttribute)}. This action does not: {action.DisplayName}.");

                    action.Filters.Add(new ProducesResponseTypeAttribute(returnType, (int)HttpStatus.Ok));
                }

                foreach (var statusCode in Enum.GetValues<HttpStatus>().Where(s => (int)s >= 400))
                {
                    switch (statusCode)
                    {
                        case HttpStatus.Ok:
                        case HttpStatus.Created:
                        case HttpStatus.NoContent:
                            continue;
                        case HttpStatus.BadRequest:
                        case HttpStatus.Unauthorized:
                        case HttpStatus.Forbidden:
                            break;
                        case HttpStatus.NotFound:
                            if (!HasHttpMethod(action, "POST"))
                                continue;
                            break;
                        case HttpStatus.Conflict:
                            if (!HasHttpMethod(action, "PUT"))
                                continue;
                            break;
                        case HttpStatus.InternalServerError:
                            break;
                        default:
                            throw new NotSupportedException($"HTTP status {statusCode} needs to be implemented.");
                    }
                    if (!IsResponseDefined(action, (int)statusCode))
                    {
                        action.Filters.Add(new ProducesResponseTypeAttribute(typeof(ProblemDetails), (int)statusCode));
                    }
                }

                if (!IsContentTypeDefined(action))
                {
                    action.Filters.Add(new ProducesAttribute(_contentType));
                }
            }
        }
    }

    private static bool IsSuccessResponseDefined(ActionModel action)
    {
        return action.ActionMethod.GetCustomAttributes(true).OfType<IApiResponseMetadataProvider>().Any(f => f.StatusCode >= 200 && f.StatusCode < 300);
    }

    private static bool IsResponseDefined(ActionModel action, int statusCode)
    {
        return action.ActionMethod.GetCustomAttributes(true).OfType<IApiResponseMetadataProvider>().Any(f => f.StatusCode == statusCode);
    }

    private static bool IsContentTypeDefined(ActionModel action)
    {
        return action.ActionMethod.GetCustomAttributes(true).OfType<ProducesAttribute>().Any(a => a.ContentTypes.Any());
    }

    private static bool HasHttpMethod(ActionModel action, params string[] methods)
    {
        return action.ActionMethod.GetCustomAttributes(true).OfType<IActionHttpMethodProvider>()
            .Any(a => a.HttpMethods.Any(method => methods.Contains(method, StringComparer.OrdinalIgnoreCase)));
    }
}