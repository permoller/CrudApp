using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace CrudApp.Infrastructure.Controllers;

public static class PropertyPathParser
{
    public static bool TryParsePropertyPath<T>(
        string path,
        [NotNullWhen(true)] out List<PropertyInfo>? propertyInfos,
        [NotNullWhen(false)] out string? error)
    {
        var propertyNames = path.Split('.');
        propertyInfos = new List<PropertyInfo>(propertyNames.Length);
        PropertyInfo? propertyInfo = null;
        foreach (var propertyName in propertyNames)
        {
            var t = propertyInfo?.PropertyType ?? typeof(T);
            propertyInfo = t.GetProperties().FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
            if (propertyInfo is null)
            {
                error = $"Property {propertyName} not found on type {t.Name}.";
                return false;
            }
            propertyInfos.Add(propertyInfo);
        }
        error = null;
        return true;
    }
}
