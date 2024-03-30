namespace CrudApp.Infrastructure.Primitives;

public static class Maybe
{
    public static Maybe<T> NoValue<T>() where T : notnull => new Maybe<T>();
}

public readonly struct Maybe<T> : IInfrastructureMaybe where T : notnull
{
    private readonly T? _value;
    private readonly bool _hasValue;

    public Maybe()
    {
        _value = default;
        _hasValue = false;
    }

    public Maybe(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        _value = value;
        _hasValue = true;
    }


    public static implicit operator Maybe<T>(T value) => new(value);

    public TOut Match<TOut>(Func<T, TOut> ifHasValue, Func<TOut> ifNoValue) => _hasValue ? ifHasValue(_value!) : ifNoValue();

    public readonly T? GetValueOrDefault() => Match(v => v, () => default(T));
    public readonly T GetValueOrDefault(T defaultValue) => Match(v => v, () => defaultValue);

    object? IInfrastructureMaybe.GetValueOrNull() => Match<object?>(v => v, () => null);
}

/// <summary>
/// This interface is only supposed to be used by the infrastructure to avoid some reflection. Use <see cref="Maybe{T}"/> in you normal code.
/// </summary>
internal interface IInfrastructureMaybe
{
    object? GetValueOrNull();
}
