using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CrudApp.Infrastructure.Database;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class EnumValueConverterAttribute : Attribute
{
    public static ValueConverter GetConverter(Type type)
    {
        return (ValueConverter)Activator.CreateInstance(typeof(EnumToStringConverter<>).MakeGenericType(type))!;
    }
}