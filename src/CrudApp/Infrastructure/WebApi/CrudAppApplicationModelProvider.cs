using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace CrudApp.Infrastructure.WebApi;

public partial class CrudAppApplicationModelProvider: IApplicationModelProvider
{
    

    public int Order => 0;

    public void OnProvidersExecuting(ApplicationModelProviderContext context)
    {
        foreach (var controller in context.Result.Controllers)
        {
            foreach (var action in controller.Actions)
            {
                action.Filters.Add(new MapperActionFilter());
                var returnType = MapperActionFilter.MapType(action);
                EnsureSuccessResponseDefined(action, returnType);
                EnsureErrorResponsesDefined(action);
            }
        }
    }

    public void OnProvidersExecuted(ApplicationModelProviderContext context)
    {
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

            action.Filters.Add(new ApiResponseMetadataProvider(returnType, returnType == typeof(void) ? HttpStatus.NoContent : HttpStatus.Ok));
            if (returnType != typeof(void) && returnType.MayTypeBeNull() == true)
                action.Filters.Add(new ApiResponseMetadataProvider(typeof(void), HttpStatus.NoContent));
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
