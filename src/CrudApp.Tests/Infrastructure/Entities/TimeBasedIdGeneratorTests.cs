using CrudApp.Infrastructure.Entities;

namespace CrudApp.Tests.Infrastructure.Entities;

public class TimeBasedIdGeneratorTests
{
    private readonly DateTimeOffset _2020 = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Ids_should_be_unique()
    {
        var numberOfGenerators = 4;
        var numberOfThreadsPerGenerator = 10;
        var numberOfIdsPerThread = 1000;
        var expectedNumberOfIds = numberOfGenerators * numberOfThreadsPerGenerator * numberOfIdsPerThread;

        var ids1 = GenerateIdsInParallel(generatorId => TimeBasedIdGenerator.NewUsingMillisecondsSince(_2020, generatorId),
                                         numberOfGenerators,
                                         numberOfThreadsPerGenerator,
                                         numberOfIdsPerThread);
        Assert.Equal(expectedNumberOfIds, ids1.Distinct().Count());

        var ids2 = GenerateIdsInParallel(generatorId => TimeBasedIdGenerator.NewUsingSecondsSince(_2020, generatorId),
                                         numberOfGenerators,
                                         numberOfThreadsPerGenerator,
                                         numberOfIdsPerThread);
        Assert.Equal(expectedNumberOfIds, ids2.Distinct().Count());
    }

    private static IReadOnlyCollection<long> GenerateIdsInParallel(Func<long, TimeBasedIdGenerator> factory, int numberOfGenerators, int numberOfThreadsPerGenerator, int numberOfIdsPerThread)
    {
        var threads = new List<Thread>();
        var listOfIdLists = new List<List<long>>();

        for (var generatorId = 0; generatorId < numberOfGenerators; generatorId++)
        {
            var generator = factory(generatorId);
            for (var threadCount = 0; threadCount < numberOfThreadsPerGenerator; threadCount++)
            {
                var idList = new List<long>();
                listOfIdLists.Add(idList);
                threads.Add(new Thread(() =>
                {
                    for (var idCount = 0; idCount < numberOfIdsPerThread; idCount++)
                        idList.Add(generator.NewId());
                }));
            }
        }
        foreach (var t in threads)
            t.Start();
        foreach (var t in threads)
            t.Join();

        var listOfAllIds = listOfIdLists.SelectMany(idList => idList).ToList();

        return listOfAllIds;
    }

    [Fact]
    public void Id_should_be_greater_than_previous_id()
    {
        var generator1 = TimeBasedIdGenerator.NewUsingMillisecondsSince(_2020);
        var generator2 = TimeBasedIdGenerator.NewUsingSecondsSince(_2020);
        foreach (var generator in new[] { generator1, generator2 })
        {
            var previousId = 0L;
            for (var i = 0; i < 5000; i++)
            {
                var currentId = generator.NewId();
                Assert.InRange(currentId, previousId + 1, long.MaxValue);
                previousId = currentId;
            }
        }
    }

    [Theory]
    [InlineData(0, 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_00000000L)]
    [InlineData(1, 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_00000001L)]
    [InlineData(2, 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_00000010L)]
    [InlineData(3, 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_00000011L)]
    public void Id_should_contain_generator_id(long generatorId, long expectedId)
    {
        var actualId = new TimeBasedIdGenerator(() => 0, generatorId, 47, 8, 8).NewId();
        Assert.Equal(expectedId, actualId);
    }

    [Fact]
    public void Id_should_contain_counter_value()
    {
        var generator = new TimeBasedIdGenerator(() => 0, 0, 47, 8, 8);
        Assert.Equal(0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_00000000L, generator.NewId());
        Assert.Equal(0b00000000_00000000_00000000_00000000_00000000_00000000_00000001_00000000L, generator.NewId());
        Assert.Equal(0b00000000_00000000_00000000_00000000_00000000_00000000_00000010_00000000L, generator.NewId());
        Assert.Equal(0b00000000_00000000_00000000_00000000_00000000_00000000_00000011_00000000L, generator.NewId());
    }

    [Fact]
    public void Id_should_contain_timestamp()
    {
        long timestamp = -1;
        var generator = new TimeBasedIdGenerator(() => ++timestamp, 0, 47, 8, 8);
        Assert.Equal(0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_00000000L, generator.NewId());
        Assert.Equal(0b00000000_00000000_00000000_00000000_00000000_00000001_00000000_00000000L, generator.NewId());
        Assert.Equal(0b00000000_00000000_00000000_00000000_00000000_00000010_00000000_00000000L, generator.NewId());
        Assert.Equal(0b00000000_00000000_00000000_00000000_00000000_00000011_00000000_00000000L, generator.NewId());
    }

    [Fact]
    public void Generator_should_wait_for_new_timestamp_if_counter_reaches_max_within_same_timestamp()
    {
        var timestamps = new long[] { 0b0001, 0b0001, 0b0001, 0b0001, 0b0001, 0b0001, 0b0001, 0b0100, 0b1111 };
        var timestampsIndex = 0;
        // 2 bits for the counter -> 4 ids can be generated with the same timestamp
        var generator = new TimeBasedIdGenerator(() => timestamps[timestampsIndex++], 0, 55, 2, 6);
        Assert.Equal(0b00000000_00000000_00000000_00000000_00000000_00000000_00000001_00000000L, generator.NewId());
        Assert.Equal(0b00000000_00000000_00000000_00000000_00000000_00000000_00000001_01000000L, generator.NewId());
        Assert.Equal(0b00000000_00000000_00000000_00000000_00000000_00000000_00000001_10000000L, generator.NewId());
        Assert.Equal(0b00000000_00000000_00000000_00000000_00000000_00000000_00000001_11000000L, generator.NewId());
        Assert.Equal(0b00000000_00000000_00000000_00000000_00000000_00000000_00000100_00000000L, generator.NewId());
    }

    [Theory]
    [InlineData(-1, true)]
    [InlineData(0, false)]
    [InlineData(3, false)]
    [InlineData(4, true)]
    [InlineData(5, true)]
    public void Generator_should_throw_exception_if_timestamp_is_out_of_range(long timestamp, bool expectException)
    {
        // configure with 2 bits for the timestamp -> max timestamp is 3
        var generator = new TimeBasedIdGenerator(() => timestamp, 0, 2, 30, 31);
        if (expectException)
            Assert.Throws<OverflowException>(() => { generator.NewId(); });
        else
            generator.NewId();
    }

    [Theory]
    [InlineData(34, 15, 15, false, false, false)] // all values are within individual limits, but sum is greater than 63
    [InlineData(32, 15, 15, false, false, false)] // all values are within individual limits, but sum is less than 63
    [InlineData(0, 32, 31, false, true, true)] // timestampSizeInBits: zero not allowed
    [InlineData(-1, 32, 32, false, true, true)] // timestampSizeInBits: negative not allowed
    [InlineData(32, -1, 32, true, false, true)] // counterSizeInBits: negative not allowed
    [InlineData(32, 32, -1, true, true, false)] // generatorIdSizeInBits: negative not allowed
    public void Generator_should_throw_if_sizeinbits_are_not_valid(int timestampSizeInBits, int counterSizeInBits, int generatorIdSizeInBits, bool expectTimestampSizeToBeValid, bool expectCounterSizeToBeValid, bool expectGeneratorIdSizeToBeBalid)
    {
        var allValid = expectTimestampSizeToBeValid && expectCounterSizeToBeValid && expectGeneratorIdSizeToBeBalid;
        if (allValid)
        {
            // should not throw
            new TimeBasedIdGenerator(() => 0, 0, timestampSizeInBits, counterSizeInBits, generatorIdSizeInBits);
        }
        else
        {
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new TimeBasedIdGenerator(() => 0, 0, timestampSizeInBits, counterSizeInBits, generatorIdSizeInBits));
            if (expectTimestampSizeToBeValid)
                Assert.DoesNotContain(nameof(timestampSizeInBits), exception.Message);
            else
                Assert.Contains(nameof(timestampSizeInBits), exception.Message);
            if (expectCounterSizeToBeValid)
                Assert.DoesNotContain(nameof(counterSizeInBits), exception.Message);
            else
                Assert.Contains(nameof(counterSizeInBits), exception.Message);
            if (expectGeneratorIdSizeToBeBalid)
                Assert.DoesNotContain(nameof(generatorIdSizeInBits), exception.Message);
            else
                Assert.Contains(nameof(generatorIdSizeInBits), exception.Message);
        }
    }
}