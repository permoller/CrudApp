using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CrudApp.Infrastructure.Database;

/// <summary>
/// Converts <see cref="DateTimeOffset" /> to and from <see cref="long"/>.
/// The UTC time is stored in the most significant bits and the offset in the least significant bits.
/// This makes it posible to correctly order the resulting longs even if the offests are different.
/// 
/// NOTE: Two <see cref="DateTimeOffset"/> representing the same UTC time but with different offsets are considered equal,
/// but when they are converted to <see cref="long"/> the they will not be equal.
/// 
/// The time is truncated to 0.1 millisecond precision.
/// The offset is truncated to 1 minute precision.
/// 
/// This implementation is base on <see cref="DateTimeOffsetToBinaryConverter"/>, but adds the support for comparing the converted values,
/// even if they use different offsets.
/// </summary>
public class DateTimeOffsetToBinaryConverterUtc : ValueConverter<DateTimeOffset, long>
{
    public DateTimeOffsetToBinaryConverterUtc()
        : base(
            v => ToLong(v),
            v => ToDateTimeOffset(v),
            null)
    {
    }

    public static DateTimeOffset ToDateTimeOffset(long v)
    {
        var utcTicks = (v >> 11) * 1000;
        var offset = TimeSpan.FromMinutes((v << 53) >> 53);
        return new DateTimeOffset(new DateTime(utcTicks + offset.Ticks, DateTimeKind.Unspecified), offset);
    }

    public static long ToLong(DateTimeOffset v)
    {
        return ((v.UtcTicks / 1000) << 11) | ((long)v.TotalOffsetMinutes & 0x7FF);
    }
}