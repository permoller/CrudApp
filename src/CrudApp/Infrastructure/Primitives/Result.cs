using System.Diagnostics.CodeAnalysis;

namespace CrudApp.Infrastructure.Primitives;

public interface IResult
{
    bool TryGetError([NotNullWhen(true)] out Error? error);
    bool TryGetValue([NotNullWhen(true)] out object? value);
}

/// <summary>
/// When a <see cref="Result{T}"/> gets returned from a controller action,
/// it is the inner value or the inner error (converted to <see cref="ProblemDetails"/>) that gets returned to the client.
/// </summary>
/// <typeparam name="T"></typeparam>
public readonly struct Result<T> : IResult where T : notnull
{
    private readonly Error? _error;
    private readonly T? _value;

    [MemberNotNullWhen(true, nameof(_error))]
    [MemberNotNullWhen(false, nameof(_value))]
    public bool HasError => _error is not null;

    [MemberNotNullWhen(true, nameof(_value))]
    [MemberNotNullWhen(false, nameof(_error))]
    public bool HasValue => !HasError;

    public bool TryGetError([NotNullWhen(true)] out Error? error)
    {
        error = _error;
        return HasError;
    }

    public bool TryGetError([NotNullWhen(true)] out Error? error, [NotNullWhen(false)] out T? value)
    {
        error = _error;
        value = _value;
        return HasError;
    }

    public bool TryGetValue([NotNullWhen(true)] out T? value)
    {
        value = _value;
        return HasValue;
    }

    public bool TryGetValue([NotNullWhen(true)] out T? value, [NotNullWhen(false)] out Error? error)
    {
        error = _error;
        value = _value;
        return HasValue;
    }

    bool IResult.TryGetValue([NotNullWhen(true)] out object? value)
    {
        value = _value;
        return HasValue;
    }

    public Result()
    {
        throw new InvalidOperationException($"{nameof(Result)} must always be initialized with a value or an error.");
    }

    public Result(Error error)
    {
        _error = error;
    }

    public Result(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        _value = value;
    }

    public static implicit operator Result<T>(T value) => new(value);
    public static implicit operator Result<T>(Error error) => new(error);
    public static implicit operator Result<T>?(Error? error) => error is null ? null : new(error);
    public static implicit operator Result<Nothing>(Result<T> result) => result.TryGetError(out var error) ? error : Nothing.Instance;

}

public static class Result
{
    public static Result<T> From<T>(T value) where T : notnull => new Result<T>(value);

    public static Result<Nothing> FromNothing() => new Result<Nothing>(Nothing.Instance);


    public static Result<T> Use<T>(this Result<T> result, Action<T> func) where T : notnull
    {
        if (result.TryGetValue(out var value))
            func(value);
        return result;
    }
    public static async Task<Result<T>> Use<T>(this Task<Result<T>> result, Action<T> func) where T : notnull
    {
        var r = await result;
        if (r.TryGetValue(out var value))
            func(value);
        return r;
    }
    public static async Task<Result<T>> Use<T>(this Task<Result<T>> result, Func<T, Task> func) where T : notnull
    {
        var r = await result;
        if (r.TryGetValue(out var value))
            await func(value);
        return r;
    }

    public static Result<T> Validate<T>(this Result<T> result, Func<T, Error?> func) where T : notnull =>
        result.TryGetError(out var error, out var value) ? error : ((Result<T>?)func(value) ?? value);
    public static async Task<Result<T>> Validate<T>(this Result<T> result, Func<T, Task<Error?>> func) where T : notnull =>
        result.TryGetError(out var error, out var value) ? error : ((Result<T>?)await func(value) ?? value);
    public static async Task<Result<T>> Validate<T>(this Task<Result<T>> result, Func<T, Error?> func) where T : notnull =>
        (await result).TryGetError(out var error, out var value) ? error : ((Result<T>?)func(value) ?? value);
    public static async Task<Result<T>> Validate<T>(this Task<Result<T>> result, Func<T, Task<Error?>> func) where T : notnull =>
        (await result).TryGetError(out var error, out var value) ? error : ((Result<T>?)await func(value) ?? value);

    public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> func) where TIn : notnull where TOut : notnull =>
        result.TryGetError(out var error, out var value) ? error : func(value);
    public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, Result<TOut>> func) where TIn : notnull where TOut : notnull =>
        result.TryGetError(out var error, out var value) ? error : func(value);
    public static async Task<Result<TOut>> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<TOut>> func) where TIn : notnull where TOut : notnull =>
        result.TryGetError(out var error, out var value) ? error : await func(value);
    public static async Task<Result<TOut>> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<Result<TOut>>> func) where TIn : notnull where TOut : notnull =>
        result.TryGetError(out var error, out var value) ? error : await func(value);
    public static async Task<Result<TOut>> Map<TIn, TOut>(this Task<Result<TIn>> result, Func<TIn, TOut> func) where TIn : notnull where TOut : notnull =>
        (await result).TryGetError(out var error, out var value) ? error : func(value);
    public static async Task<Result<TOut>> Map<TIn, TOut>(this Task<Result<TIn>> result, Func<TIn, Result<TOut>> func) where TIn : notnull where TOut : notnull =>
        (await result).TryGetError(out var error, out var value) ? error : func(value);
    public static async Task<Result<TOut>> Map<TIn, TOut>(this Task<Result<TIn>> result, Func<TIn, Task<TOut>> func) where TIn : notnull where TOut : notnull =>
        (await result).TryGetError(out var error, out var value) ? error : await func(value);
    public static async Task<Result<TOut>> Map<TIn, TOut>(this Task<Result<TIn>> result, Func<TIn, Task<Result<TOut>>> func) where TIn : notnull where TOut : notnull =>
        (await result).TryGetError(out var error, out var value) ? error : await func(value);
    
}