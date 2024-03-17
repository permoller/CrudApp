using CrudApp.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Globalization;


namespace CrudApp.Tests.Infrastructure.Database;
public class DateTimeOffsetToBinaryConverterUtcTest
{
    private readonly ValueConverter<DateTimeOffset, long> _converter = new DateTimeOffsetToBinaryConverterUtc();
    
    [Theory]
    [InlineData("9999-12-31 23:59:59 +00:00")]// Max DateTimeOffset
    [InlineData("0001-01-01 00:00:00 +00:00")]// Min DateTimeOffset
    [InlineData("2024-03-17 10:20:00 +14:00")]// Max positive offset
    [InlineData("2024-03-17 10:20:00 +00:01")]// Min positive offset
    [InlineData("2024-03-17 10:20:00 -14:00")]// Max negative offset
    [InlineData("2024-03-17 10:20:00 -00:01")]// Min negative offset
    
    public void ConvertToAndFromShouldBeEqual(string input)
    {
        var dto = DateTimeOffset.Parse(input, CultureInfo.InvariantCulture);
        Assert.Equal(dto, _converter.ConvertFromProviderTyped(_converter.ConvertToProviderTyped(dto)), TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public void OrderingShouldWorkAccrosssDifferentOffsets()
    {
        var orderedDateTimeOffsets = new[] {
            DateTimeOffset.Parse("0001-01-01 00:00:00 +00:00", CultureInfo.InvariantCulture),
            DateTimeOffset.Parse("0001-01-01 00:00:00 -00:01", CultureInfo.InvariantCulture),
            DateTimeOffset.Parse("2024-03-17 10:20:00 +14:00", CultureInfo.InvariantCulture),
            DateTimeOffset.Parse("2024-03-17 10:20:00 +00:00", CultureInfo.InvariantCulture),
            DateTimeOffset.Parse("2024-03-17 10:20:00 -14:00", CultureInfo.InvariantCulture),
            DateTimeOffset.Parse("9999-12-31 23:59:59 +00:01", CultureInfo.InvariantCulture),
            DateTimeOffset.Parse("9999-12-31 23:59:59 +00:00", CultureInfo.InvariantCulture),
        };
        // Check that the test data is actually ordered correctly
        Assert.Equal(orderedDateTimeOffsets, orderedDateTimeOffsets.Order());

        var orderedLongs = orderedDateTimeOffsets.Select(dto => _converter.ConvertToProviderTyped(dto)).ToList();
        Assert.Equal(orderedLongs, orderedLongs.Order());
        orderedLongs.Reverse();
        Assert.Equal(orderedLongs, orderedLongs.OrderDescending());
    }

    [Fact]
    public void EqualityWithDifferentOffsetsDoesNotWork()
    {
        var utcTime = DateTimeOffset.Parse("2024-03-17 10:20:00 +00:00", CultureInfo.InvariantCulture);
        var time1 = new DateTimeOffset(utcTime.DateTime.AddHours(1), TimeSpan.FromHours(1));
        var time2 = new DateTimeOffset(utcTime.DateTime.AddHours(-1), TimeSpan.FromHours(-1));
        Assert.Equal(time1, time2);
        var time1Converted = _converter.ConvertToProviderTyped(time1);
        var time2Converted = _converter.ConvertToProviderTyped(time2);
        // because the different offsets are encoded into the converted values, the values will not be the same
        Assert.NotEqual(time1Converted, time2Converted);
        var time1ConvertedBack = _converter.ConvertFromProviderTyped(time1Converted);
        var time2ConvertedBack = _converter.ConvertFromProviderTyped(time2Converted);
        Assert.Equal(time1ConvertedBack, time2ConvertedBack);
    }

    [Fact]
    public void DateTimeOffsetRange()
    {
        // DateTime.MinValue with a positive offset represents a time before DateTime.MinValue UTC.
        // This is outside the allowed range.
        Assert.Throws<ArgumentOutOfRangeException>(() => new DateTimeOffset(DateTime.MinValue, TimeSpan.FromMinutes(1)));

        // DateTime.MaxValue with a negative offset represents a time after DateTime.MaxValue UTC.
        // This is outside the allowed range.
        Assert.Throws<ArgumentOutOfRangeException>(() => new DateTimeOffset(DateTime.MaxValue, TimeSpan.FromMinutes(-1)));
    }

    [Fact]
    public void Default()
    {
        Assert.Equal(0l, _converter.ConvertToProvider(DateTimeOffset.MinValue));
        Assert.Equal(0l, _converter.ConvertToProvider(new DateTimeOffset()));
        Assert.Equal(new DateTimeOffset(), _converter.ConvertFromProvider(0));
    }

    [Fact]
    public void Null()
    {
        Assert.Null(_converter.ConvertToProvider(null));
        Assert.Null(_converter.ConvertFromProvider(null));
    }
}
