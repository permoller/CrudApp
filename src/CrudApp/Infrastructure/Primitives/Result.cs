using System.Diagnostics.CodeAnalysis;

namespace CrudApp.Infrastructure.Primitives;

public interface IResult
{
    bool TryGetError([MaybeNullWhen(false)] out Error error);
    bool TryGetValue(out object? value);
}

public readonly struct Result<T> : IResult
{
    public T? ValueOrDefault { get; }
    public Error? Error { get; }

    private Result(T? valueOrDefault, Error? error)
    {
        ValueOrDefault = valueOrDefault;
        Error = error;
    }

    public static implicit operator Result<T>(T value) => new(value, null);

    public static implicit operator Result<T>(Error error) => new(default, error);

    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsError => Error.HasValue;

    public bool IsSuccess => !IsError;

    public bool TryGetError([MaybeNullWhen(false)] out Error error)
    {
        error = Error.GetValueOrDefault();
        return IsError;
    }

    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        value = ValueOrDefault;
        return IsSuccess;
    }
    bool IResult.TryGetValue(out object? value)
    {
        value = ValueOrDefault;
        return IsSuccess;
    }

    public Result<Nothing> Select(Action<T> selector) =>
        IsError ? Error.Value : Nothing.Select(ValueOrDefault!, selector);

    public async Task<Result<Nothing>> Select(Func<T, Task> selector) =>
        IsError ? Error.Value : await Nothing.Select(ValueOrDefault!, selector);

    public Result<T2> Select<T2>(Func<T, T2> selector) =>
        IsError ? Error.Value : selector(ValueOrDefault!);

    public async Task<Result<T2>> Select<T2>(Func<T, Task<T2>> selector) =>
        IsError ? Error.Value : await selector(ValueOrDefault!);

    public Result<T2> Select<T2>(Func<T, Result<T2>> selector) =>
        IsError ? Error.Value : selector(ValueOrDefault!);

    public async Task<Result<T2>> Select<T2>(Func<T, Task<Result<T2>>> selector) =>
        IsError ? Error.Value : await selector(ValueOrDefault!);

    
}