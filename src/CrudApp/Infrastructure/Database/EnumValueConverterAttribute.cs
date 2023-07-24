using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Collections.Concurrent;

namespace CrudApp.Infrastructure.Database;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class EnumValueConverterAttribute : Attribute
{
    private static ConcurrentDictionary<Type, ValueConverter> _cache = new ConcurrentDictionary<Type, ValueConverter>();

    public static ValueConverter GetConverter(Type type)
    {
        return _cache.GetOrAdd(type, t => (ValueConverter)Activator.CreateInstance(typeof(EnumValueConverter<>).MakeGenericType(type)));
    }

    private sealed class EnumValueConverter<T> : ValueConverter<T, string>
    {
        public EnumValueConverter() : base(
            v => v.ToString(),
            v => (T)Enum.Parse(typeof(T), v, true),
            convertsNulls: false)
        {
        }
    }
}