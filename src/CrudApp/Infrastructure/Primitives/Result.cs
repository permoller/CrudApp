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
public readonly struct Result<T> : IResult
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

    public TOut Match<TOut>(Func<T, TOut> valueFunc, Func<Error, TOut> errorFunc)
    {
        if (TryGetValue(out var value, out var error))
            return valueFunc(value);
        return errorFunc(error);
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
    public static Result<T> From<T>(T value) => new Result<T>(value);

    public static Result<Nothing> FromNothing() => new Result<Nothing>(Nothing.Instance);


    public static Result<T> Use<T>(this Result<T> result, Action<T> func) => 
        result.Select(v => { func(v); return v; });
    public static async Task<Result<T>> Use<T>(this Result<T> result, Func<T, Task> func) =>
        await result.Select(async v => { await func(v); return v; });
    public static async Task<Result<T>> Use<T>(this Task<Result<T>> result, Action<T> func) =>
        (await result).Select(v => { func(v); return v; });
    public static async Task<Result<T>> Use<T>(this Task<Result<T>> result, Func<T, Task> func) =>
        await (await result).Select(async v => { await func(v); return v; });

    public static Result<T> Validate<T>(this Result<T> result, Func<T, Error?> func) =>
        result.Select(v => (Result<T>?)func(v) ?? result);
    public static async Task<Result<T>> Validate<T>(this Result<T> result, Func<T, Task<Error?>> func) =>
        await result.Select(async v => (Result<T>?)await func(v) ?? result);
    public static async Task<Result<T>> Validate<T>(this Task<Result<T>> result, Func<T, Error?> func) =>
        (await result).Validate(func);
    public static async Task<Result<T>> Validate<T>(this Task<Result<T>> result, Func<T, Task<Error?>> func) =>
        await (await result).Validate(func);

    public static Result<TOut> Select<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> select) =>
        result.Match(v => new Result<TOut>(select(v)), e => new Result<TOut>(e));
    public static Result<TOut> Select<TIn, TOut>(this Result<TIn> result, Func<TIn, Result<TOut>> select) =>
        result.Match(v => select(v), e => new Result<TOut>(e));
    public static Task<Result<TOut>> Select<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<TOut>> select) =>
        result.Match(async v => new Result<TOut>(await select(v)), e => Task.FromResult(new Result<TOut>(e)));
    public static Task<Result<TOut>> Select<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<Result<TOut>>> select) =>
        result.Match(v => select(v), e => Task.FromResult(new Result<TOut>(e)));

    public static async Task<Result<TOut>> Select<TIn, TOut>(this Task<Result<TIn>> result, Func<TIn, TOut> select) =>
        (await result).Select(select);
    public static async Task<Result<TOut>> Select<TIn, TOut>(this Task<Result<TIn>> result, Func<TIn, Result<TOut>> select) =>
        (await result).Select(select);
    public static async Task<Result<TOut>> Select<TIn, TOut>(this Task<Result<TIn>> result, Func<TIn, Task<TOut>> select) =>
        await (await result).Select(select);
    public static async Task<Result<TOut>> Select<TIn, TOut>(this Task<Result<TIn>> result, Func<TIn, Task<Result<TOut>>> select) =>
        await (await result).Select(select);

    public static Result<TOut> SelectMany<TInA, TInB, TOut>(this Result<TInA> resultA, Func<TInA, Result<TInB>> selectB, Func<TInA, TInB, TOut> select) =>
        resultA.Select(a => selectB(a).Select(b => select(a, b)));
    public static Result<TOut> SelectMany<TInA, TInB, TOut>(this Result<TInA> resultA, Func<TInA, Result<TInB>> selectB, Func<TInA, TInB, Result<TOut>> select) =>
        resultA.Select(a => selectB(a).Select(b => select(a, b)));
    public static async Task<Result<TOut>> SelectMany<TInA, TInB, TOut>(this Result<TInA> resultA, Func<TInA, Result<TInB>> selectB, Func<TInA, TInB, Task<TOut>> select) =>
        await resultA.Select(a => selectB(a).Select(async b => await select(a, b)));
    public static async Task<Result<TOut>> SelectMany<TInA, TInB, TOut>(this Result<TInA> resultA, Func<TInA, Result<TInB>> selectB, Func<TInA, TInB, Task<Result<TOut>>> select) =>
        await resultA.Select(a => selectB(a).Select(async b => await select(a, b)));

    public static async Task<Result<TOut>> SelectMany<TInA, TInB, TOut>(this Task<Result<TInA>> taskResultA, Func<TInA, Result<TInB>> selectB, Func<TInA, TInB, TOut> select) =>
        (await taskResultA).SelectMany(selectB, select);
    public static async Task<Result<TOut>> SelectMany<TInA, TInB, TOut>(this Task<Result<TInA>> taskResultA, Func<TInA, Result<TInB>> selectB, Func<TInA, TInB, Result<TOut>> select) =>
        (await taskResultA).SelectMany(selectB, select);
    public static async Task<Result<TOut>> SelectMany<TInA, TInB, TOut>(this Task<Result<TInA>> taskResultA, Func<TInA, Result<TInB>> selectB, Func<TInA, TInB, Task<TOut>> select) =>
        await (await taskResultA).SelectMany(selectB, select);
    public static async Task<Result<TOut>> SelectMany<TInA, TInB, TOut>(this Task<Result<TInA>> taskResultA, Func<TInA, Result<TInB>> selectB, Func<TInA, TInB, Task<Result<TOut>>> select) =>
        await (await taskResultA).SelectMany(selectB, select);

    public static async Task<Result<TOut>> SelectMany<TInA, TInB, TOut>(this Result<TInA> resultA, Func<TInA, Task<Result<TInB>>> selectB, Func<TInA, TInB, TOut> select) =>
        await resultA.Select(async a => (await selectB(a)).Select(b => select(a, b)));
    public static async Task<Result<TOut>> SelectMany<TInA, TInB, TOut>(this Result<TInA> resultA, Func<TInA, Task<Result<TInB>>> selectB, Func<TInA, TInB, Result<TOut>> select) =>
        await resultA.Select(async a => (await selectB(a)).Select(b => select(a, b)));
    public static async Task<Result<TOut>> SelectMany<TInA, TInB, TOut>(this Result<TInA> resultA, Func<TInA, Task<Result<TInB>>> selectB, Func<TInA, TInB, Task<TOut>> select) =>
        await resultA.Select(async a => await (await selectB(a)).Select(async b => await select(a, b)));
    public static async Task<Result<TOut>> SelectMany<TInA, TInB, TOut>(this Result<TInA> resultA, Func<TInA, Task<Result<TInB>>> selectB, Func<TInA, TInB, Task<Result<TOut>>> select) =>
        await resultA.Select(async a => await (await selectB(a)).Select(async b => await select(a, b)));

    public static async Task<Result<TOut>> SelectMany<TInA, TInB, TOut>(this Task<Result<TInA>> taskResultA, Func<TInA, Task<Result<TInB>>> selectB, Func<TInA, TInB, TOut> select) =>
        await (await taskResultA).SelectMany(selectB, select);
    public static async Task<Result<TOut>> SelectMany<TInA, TInB, TOut>(this Task<Result<TInA>> taskResultA, Func<TInA, Task<Result<TInB>>> selectB, Func<TInA, TInB, Result<TOut>> select) =>
        await (await taskResultA).SelectMany(selectB, select);
    public static async Task<Result<TOut>> SelectMany<TInA, TInB, TOut>(this Task<Result<TInA>> taskResultA, Func<TInA, Task<Result<TInB>>> selectB, Func<TInA, TInB, Task<TOut>> select) =>
        await (await taskResultA).SelectMany(selectB, select);
    public static async Task<Result<TOut>> SelectMany<TInA, TInB, TOut>(this Task<Result<TInA>> taskResultA, Func<TInA, Task<Result<TInB>>> selectB, Func<TInA, TInB, Task<Result<TOut>>> select) =>
        await (await taskResultA).SelectMany(selectB, select);
}