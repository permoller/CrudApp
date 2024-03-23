namespace CrudApp.Infrastructure.Entities;

/// <summary>
/// Generates 63 bit positive integers containing a timestamp as the most significant bits, followed
/// by a counter, allowing multiple ids to be generated within the same timestamp,
/// and a generator id, allowing multiple simultaious generators with different ids to co-exists.
/// </summary>
public class TimeBasedIdGenerator
{
    private readonly object _lock;
    private readonly int _timestampSizeInBits;
    private readonly int _generatorIdSizeInBits;
    private readonly int _counterSizeInBits;
    private readonly Func<long> _getTimestamp;
    private long? _lastTimestamp;
    private readonly long _generatorId;
    private long _counter;

    /// <summary>
    /// Creates a generator that use the milliseconds since <paramref name="start"/> as the
    /// timestamp in the generated IDs.
    /// <para>
    /// The default values for <paramref name="timestampSizeInBits"/> (41), <paramref
    /// name="counterSizeInBits"/> (12) and <paramref name="generatorIdSizeInBits"/> (10) allows
    /// having 1024 generators generating 4096 ids per millisecond for close to 70 years after
    /// <paramref name="start"/>.
    /// </para>
    /// </summary>
    public static TimeBasedIdGenerator NewUsingMillisecondsSince(DateTimeOffset start, long generatorId = 0L, int timestampSizeInBits = 41, int counterSizeInBits = 12, int generatorIdSizeInBits = 10)
    {
        var startTicks = start.UtcTicks;
        var getTicks = () => (DateTimeOffset.UtcNow.Ticks - startTicks) / TimeSpan.TicksPerMillisecond;
        return new TimeBasedIdGenerator(getTicks, generatorId, timestampSizeInBits, counterSizeInBits, generatorIdSizeInBits);
    }

    /// <summary>
    /// Creates a generator that use the seconds since <paramref name="start"/> as the timestamp in
    /// the generated IDs.
    /// <para>
    /// The default values for <paramref name="timestampSizeInBits"/> (31), <paramref
    /// name="counterSizeInBits"/> (22) and <paramref name="generatorIdSizeInBits"/> (10) allows
    /// having 1024 generators generating 4194304 ids per second for close to 70 years after
    /// <paramref name="start"/>.
    /// </para>
    /// </summary>
    public static TimeBasedIdGenerator NewUsingSecondsSince(DateTimeOffset start, long generatorId = 0L, int timestampSizeInBits = 31, int counterSizeInBits = 22, int generatorIdSizeInBits = 10)
    {
        var startTicks = start.UtcTicks;
        var getTicks = () => (DateTimeOffset.UtcNow.Ticks - startTicks) / (TimeSpan.TicksPerMillisecond * 1000);
        return new TimeBasedIdGenerator(getTicks, generatorId, timestampSizeInBits, counterSizeInBits, generatorIdSizeInBits);
    }

    /// <summary>
    /// </summary>
    /// <param name="getTimestamp">
    /// Make sure the the resolution of the timer is high enough to ensure we do not get the same
    /// timestamp before and after the generator is recreated. There is an internal counter ensuring
    /// we can generate multiple ids within the same timestamp. If for example the executing process
    /// is restarted, the counter will be reset. If the timestamp before and after the restart is
    /// the same the generated ids will not be unique.
    /// </param>
    /// <param name="generatorId"></param>
    /// <param name="timestampSizeInBits"></param>
    /// <param name="counterSizeInBits"></param>
    /// <param name="generatorIdSizeInBits"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public TimeBasedIdGenerator(Func<long> getTimestamp, long generatorId, int timestampSizeInBits, int counterSizeInBits, int generatorIdSizeInBits)
    {
        if (timestampSizeInBits <= 0)
            throw new ArgumentOutOfRangeException(nameof(timestampSizeInBits));
        _timestampSizeInBits = timestampSizeInBits;

        if (counterSizeInBits < 0)
            throw new ArgumentOutOfRangeException(nameof(counterSizeInBits));
        _counterSizeInBits = counterSizeInBits;

        if (generatorIdSizeInBits < 0)
            throw new ArgumentOutOfRangeException(nameof(generatorIdSizeInBits));
        _generatorIdSizeInBits = generatorIdSizeInBits;

        if (timestampSizeInBits + counterSizeInBits + generatorIdSizeInBits != 63)
            throw new ArgumentOutOfRangeException(
                $"The sum of" +
                $" {nameof(timestampSizeInBits)} ({timestampSizeInBits})" +
                $", {nameof(counterSizeInBits)} ({counterSizeInBits})" +
                $" and {nameof(generatorIdSizeInBits)} ({generatorIdSizeInBits})" +
                $" must be 63.", (Exception?)null);

        if (generatorId < 0L || generatorId >= 1L << generatorIdSizeInBits)
            throw new ArgumentOutOfRangeException(nameof(generatorId));
        _generatorId = generatorId;

        _lock = new object();
        _getTimestamp = getTimestamp;
    }

    public long NewId()
    {
        lock (_lock)
        {
            var timestamp = _getTimestamp();
            if (timestamp < 0)
                throw new OverflowException($"The timestamp {timestamp} must not be negative.");

            if (timestamp >= 1L << _timestampSizeInBits)
                throw new OverflowException($"The timestamp {timestamp} does not fit in the {_timestampSizeInBits} bits allocated for it.");

            // if time is moving backwards continue using the last used timestamp
            if (_lastTimestamp.HasValue && timestamp < _lastTimestamp.Value)
                timestamp = _lastTimestamp.Value;

            _counter = _lastTimestamp.HasValue && timestamp == _lastTimestamp.Value ? _counter + 1 : 0;

            // if we reached the maximum number of ids within the current timestamp
            if (_counter >= 1L << _counterSizeInBits)
            {
                // reset counter and wait for the next timestamp
                _counter = 0;
                while (timestamp <= _lastTimestamp)
                    timestamp = _getTimestamp();
            }
            _lastTimestamp = timestamp;

            var id = timestamp;
            id <<= _counterSizeInBits;
            id |= _counter;
            id <<= _generatorIdSizeInBits;
            id |= _generatorId;
            return id;
        }
    }

    public long GetTimestampFromId(long id) =>
        id >> _counterSizeInBits + _generatorIdSizeInBits;

    public long GetGeneratorIdFromId(long id) =>
        id << (64 - _generatorIdSizeInBits) >> (64 - _generatorIdSizeInBits);
}