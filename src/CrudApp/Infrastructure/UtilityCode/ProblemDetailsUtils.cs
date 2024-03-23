using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace CrudApp.Infrastructure.UtilityCode;

public static class ProblemDetailsUtils
{
    public static bool TryGetErrors(this ProblemDetails problemDetails, [NotNullWhen(true)] out Dictionary<string, string[]>? errors) =>
        problemDetails.TryGetExtension("errors", out errors);

    public static bool TryGetData(this ProblemDetails problemDetails, [NotNullWhen(true)] out Dictionary<string, object?>? data) =>
       problemDetails.TryGetExtension("data", out data);

    public static bool TryGetExtension<T>(this ProblemDetails problemDetails, string key, [NotNullWhen(true)] out T? value)
    {
        var obj = problemDetails.Extensions.FirstOrDefault(kvp => StringComparer.OrdinalIgnoreCase.Equals(key, kvp.Key)).Value;
        if (obj is not null)
        {
            if (obj is T t)
            {
                value = t;
                return true;
            }
            if (obj is JsonElement jsonElement)
            {
                try
                {
                    value = jsonElement.Deserialize<T>(JsonUtils.ApiJsonSerializerOptions);
                    return value is not null;
                }
                catch
                {
                    // ignore parsing errors
                }
            }
        }
        value = default;
        return false;
    }
}
