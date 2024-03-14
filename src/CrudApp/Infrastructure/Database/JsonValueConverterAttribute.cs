using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace CrudApp.Infrastructure.Database;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class JsonValueConverterAttribute : Attribute
{
    public static ValueConverter GetConverter(Type type)
    {
        return (ValueConverter)Activator.CreateInstance(typeof(JsonValueConverter<>).MakeGenericType(type));
    }

    private sealed class JsonValueConverter<T> : ValueConverter<T, string>
    {
        public JsonValueConverter() : base(
            v => JsonSerializer.Serialize(v!, JsonUtils.DbJsonSerializerOptions)!,
            v => JsonSerializer.Deserialize<T>(v!, JsonUtils.DbJsonSerializerOptions)!,
            convertsNulls: false)
        {
        }
    }
}