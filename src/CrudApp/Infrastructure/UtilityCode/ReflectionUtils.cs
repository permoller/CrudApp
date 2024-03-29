﻿using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace CrudApp.Infrastructure.UtilityCode;

public static class ReflectionUtils
{
    public static T2 Return<T1, T2>(this T1 _, T2 x) => x;

    public static IEnumerable<Type> GetAllTypesInApplication()
        => AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name?.StartsWith("CrudApp") == true).SelectMany(a => a.GetTypes());

    public static IEnumerable<Type> GetSubclassesInApplication(this Type? baseClass, bool includeAbstractClasses = false)
    {
        if (baseClass is null)
            return Enumerable.Empty<Type>();

        var allTypes = GetAllTypesInApplication();
        if (!includeAbstractClasses)
            allTypes = allTypes.Where(t => !t.IsAbstract);

        if (baseClass.IsGenericTypeDefinition)
            return allTypes.Where(t => t.IsClass && t.IsSubtypeOfGenericTypeDefinition(baseClass));
        return allTypes.Where(t => t.IsSubclassOf(baseClass));
    }

    public static bool IsSubtypeOfGenericTypeDefinition(this Type? type, Type genericTypeDefinition)
        => type.FindGenericArgumentsForGenericTypeDefinition(genericTypeDefinition) is not null;

    /// <summary>
    /// Searches through the type heirachy looking for a generic type that uses <paramref name="genericTypeDefinition"/> as the generic type definition.
    /// Returns the arguments of the first match or null if no match is found.
    /// </summary>
    public static Type[]? FindGenericArgumentsForGenericTypeDefinition(this Type? type, Type genericTypeDefinition)
    {
        if (!genericTypeDefinition.IsGenericTypeDefinition)
            throw new ArgumentException($"{nameof(genericTypeDefinition)} must be a generic type definition.");

        var typesToCheck = new Queue<Type?>();
        typesToCheck.Enqueue(type);
        Type? typeToCheck;
        while (typesToCheck.Count > 0)
        {
            typeToCheck = typesToCheck.Dequeue();

            if (typeToCheck is null || typeToCheck == typeof(object))
                continue;

            if (typeToCheck.IsGenericType && typeToCheck.GetGenericTypeDefinition() == genericTypeDefinition)
                return typeToCheck.GetGenericArguments();

            typesToCheck.Enqueue(typeToCheck.BaseType);
            foreach (var i in typeToCheck.GetInterfaces())
                typesToCheck.Enqueue(i);
        }
        return null;
    }

    /// <summary>
    /// Like <see cref="FindGenericArgumentsForGenericTypeDefinition(Type?, Type)"/> but returns false if no type arguments are found.
    /// </summary>
    public static bool TryGetGenericArgumentsForGenericTypeDefinition(this Type? type, Type genericTypeDefinition, [NotNullWhen(true)] out Type[]? genericArguments)
    {
        genericArguments = type.FindGenericArgumentsForGenericTypeDefinition(genericTypeDefinition);
        return genericArguments is not null;
    }

    /// <summary>
    /// Like <see cref="FindGenericArgumentsForGenericTypeDefinition(Type?, Type)"/> but throws an <see cref="ArgumentException"/> if no type arguments are found.
    /// </summary>
    public static Type[] GetGenericArgumentsForGenericTypeDefinition(this Type? type, Type genericTypeDefinition)
    {
        return type.FindGenericArgumentsForGenericTypeDefinition(genericTypeDefinition) 
            ?? throw new ArgumentException($"{type?.Name} does not inherit from {genericTypeDefinition.Name}.");
    }

    public static bool HasAttribute<T>(this PropertyInfo? propertyInfo) where T : Attribute
        => propertyInfo?.GetCustomAttribute<T>(true) is not null;

    public static bool HasAttribute<T>(this Type? type) where T : Attribute
        => type?.GetCustomAttribute<T>(true) is not null;

    /// <summary>
    /// Reference types and <see cref="Nullable{T}"/> may be null.
    /// Other value types may not be null.
    /// </summary>
    public static bool? MayTypeBeNull(this Type? type)
    {
        if (type is null)
            return null;

        // Reference types may be null
        if (!type.IsValueType)
            return true;

        // Nullable<T> is the only value type that may be null
        var isNullableStruct = type.IsGenericType && !type.IsGenericTypeDefinition && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        return isNullableStruct;
    }

    private static readonly NullabilityInfoContext _nullabilityInfoContext = new();

    /// <summary>
    /// Reference types marked with nullable and <see cref="Nullable{T}"/> may be null.
    /// Other value types and reference types that are not marked as nullable may not be null.
    /// </summary>
    public static bool? MayPropertyBeNull(this PropertyInfo? propertyInfo)
    {
        if (propertyInfo is null)
            return null;

        if (propertyInfo.PropertyType.MayTypeBeNull() == false)
            return false;

        // Look for reference nullability attributes
        var nullabilityState = _nullabilityInfoContext.Create(propertyInfo).ReadState;
        return nullabilityState switch
        {
            NullabilityState.NotNull => false,
            NullabilityState.Nullable => true,
            _ => null
        };
    }

    
    public static Result<List<PropertyInfo>> ParsePropertyPath(this Type type, string path)
    {
        var propertyNames = path.Split('.');
        var propertyInfos = new List<PropertyInfo>(propertyNames.Length);
        PropertyInfo? propertyInfo = null;
        foreach (var propertyName in propertyNames)
        {
            var t = propertyInfo?.PropertyType ?? type;
            propertyInfo = Array.Find(t.GetProperties(), p => p.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
            if (propertyInfo == default)
            {
                return new Error.PropertyNotFoundOnType(propertyName, t);
            }
            propertyInfos.Add(propertyInfo);
        }
        return propertyInfos;
    }

    public static object? GetDefault(this Type type)
    {
        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }
        return null;
    }
}
