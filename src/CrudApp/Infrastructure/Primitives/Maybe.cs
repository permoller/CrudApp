using System.Diagnostics.CodeAnalysis;

namespace CrudApp.Infrastructure.Primitives;

public readonly struct Maybe<T> : IMaybe
{
    private readonly T? _value;

    [MemberNotNullWhen(true, nameof(Value), nameof(_value))]
    public bool HasValue { get; }

    public T Value => HasValue ? _value : throw new InvalidOperationException($"{nameof(Maybe<T>)} hos no {nameof(Value)}.");
    object? IMaybe.Value => Value;


    public Maybe(T? value)
    {
        _value = value;
        HasValue = value is not null;
    }
    
    public static implicit operator Maybe<T>(T? value) => new(value);

    public static explicit operator T(Maybe<T> maybe) => maybe.Value;

    public static Maybe<T> NoValue => default;


    public readonly T? GetValueOrDefault() => _value;

    public readonly T? GetValueOrDefault(T defaultValue) =>
        HasValue ? _value : defaultValue;
}

public static class Maybe
{
    public static Maybe<T> From<T>(T? obj)
        => obj is not null ? new Maybe<T>(obj) : Maybe<T>.NoValue;
}

/// <summary>
/// This interface is only supposed to be used by the infrastructure to avoid some reflection. Use <see cref="Maybe{T}"/> in you normal code.
/// </summary>
internal interface IMaybe
{
    [MemberNotNullWhen(true, nameof(Value))]
    bool HasValue { get; }
    object? Value { get; }
}
