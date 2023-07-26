using System.Reflection;

namespace CrudApp.Infrastructure.Query;

public static class PropertyPathHelper
{
    public static List<PropertyInfo> ParsePropertyPath<T>(string path)
    {
        if(string.IsNullOrWhiteSpace(path))
            throw new ApiResponseException(HttpStatus.BadRequest, "Missing property in filter.");
        var propertyNames = path.Split('.');
        var propertyInfos = new List<PropertyInfo>(propertyNames.Length);
        PropertyInfo? propertyInfo = null;
        foreach (var propertyName in propertyNames)
        {
            var t = propertyInfo?.PropertyType ?? typeof(T);
            propertyInfo = Array.Find(t.GetProperties(), p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
            if (propertyInfo == default)
            {
                throw new ApiResponseException(HttpStatus.BadRequest, $"Property {propertyName} not found on type {t.Name}.");
            }
            propertyInfos.Add(propertyInfo);
        }
        return propertyInfos;
    }
}
