using System.Reflection;

namespace CrudApp.Infrastructure.UtilityCode;

public static class ReflectionUtils
{
    public static IEnumerable<Type> GetAllTypes()
        => AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes());

    public static IEnumerable<Type> GetSubclasses(this Type baseClass, bool includeAbstractClasses = false)
        => GetAllTypes().Where(t => t.IsSubclassOf(baseClass) && (includeAbstractClasses || !t.IsAbstract));

    public static bool HasAttribute<T>(this PropertyInfo? propertyInfo) where T : Attribute
        => propertyInfo?.GetCustomAttribute<T>(true) is not null;

    public static bool HasAttribute<T>(this Type? type) where T : Attribute
        => type?.GetCustomAttribute<T>(true) is not null;

    private static readonly NullabilityInfoContext _nullabilityInfoContext = new();

    public static bool MayBeNull(this PropertyInfo? propertyInfo)
        => propertyInfo?.GetNullabilityState() == NullabilityState.Nullable;

    public static bool MayNotBeNull(this PropertyInfo? propertyInfo)
        => propertyInfo?.GetNullabilityState() == NullabilityState.NotNull;

    private static NullabilityState GetNullabilityState(this PropertyInfo? propertyInfo)
    {
        if (propertyInfo is null)
            return NullabilityState.Unknown;

        // Nullable<T> may always be null
        if (Nullable.GetUnderlyingType(propertyInfo.PropertyType) is not null)
            return NullabilityState.Nullable;

        // Other value types may not be null
        if (propertyInfo.PropertyType.IsValueType)
            return NullabilityState.NotNull;

        // Look for reference nullability attributes
        return _nullabilityInfoContext.Create(propertyInfo).ReadState;
    }

    public static List<PropertyInfo> ParsePropertyPath(this Type type, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ApiResponseException(HttpStatus.BadRequest, "Missing property in filter.");
        var propertyNames = path.Split('.');
        var propertyInfos = new List<PropertyInfo>(propertyNames.Length);
        PropertyInfo? propertyInfo = null;
        foreach (var propertyName in propertyNames)
        {
            var t = propertyInfo?.PropertyType ?? type;
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
