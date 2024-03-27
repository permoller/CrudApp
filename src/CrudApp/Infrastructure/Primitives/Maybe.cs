using System.Diagnostics.CodeAnalysis;

namespace CrudApp.Infrastructure.Primitives;

public readonly struct Maybe<T> : IMaybe where T : notnull
{
    private readonly T? _value;

    [MemberNotNullWhen(true, nameof(_value))]
    private bool HasValue { get; }

    public TOut Match<TOut>(Func<T, TOut> ifHasValue, Func<TOut> ifNoValue) =>
        HasValue ? ifHasValue(_value) : ifNoValue();

    TOut IMaybe.Match<TOut>(Func<object?, TOut> ifHasValue, Func<TOut> ifNoValue) =>
        Match(value => ifHasValue(value), ifNoValue);

    public Maybe()
    {
        _value = default;
        HasValue = false;
    }

    public Maybe(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        _value = value;
        HasValue = true;
    }


    public static implicit operator Maybe<T>(T value) => new(value);

    public readonly T GetValueOrDefault(T defaultValue) =>
        HasValue ? _value : defaultValue;
}

public static class Maybe
{
    public static Maybe<T> NoValue<T>() where T : notnull => default;
}
public static class MaybeObject
{
    public static Maybe<T> ToMaybe<T>(this T? refValue) where T : class => refValue is null ? Maybe.NoValue<T>() : new(refValue);
    
}
public static class MaybeStruct
{
    public static Maybe<T> ToMaybe<T>(this T structValue) where T : struct => new(structValue);
    public static Maybe<T> ToMaybe<T>(this T? nullableStructValue) where T : struct => nullableStructValue is null ? Maybe.NoValue<T>() : new(nullableStructValue.Value);
}

/// <summary>
/// This interface is only supposed to be used by the infrastructure to avoid some reflection. Use <see cref="Maybe{T}"/> in you normal code.
/// </summary>
internal interface IMaybe
{
    TOut Match<TOut>(Func<object?, TOut> ifHasValue, Func<TOut> ifNoValue);
}
