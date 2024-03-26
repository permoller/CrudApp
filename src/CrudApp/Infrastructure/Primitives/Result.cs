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


    #region Methods to support using Result<T> in LINQ

    public static Result<B> Select<A, B>(this Result<A> resultA, Func<A, B> selectB) =>
        resultA.Match(valueA => new Result<B>(selectB(valueA)), errorA => new Result<B>(errorA));
    public static Result<B> Select<A, B>(this Result<A> resultA, Func<A, Result<B>> selectResultB) =>
        resultA.Match(valueA => selectResultB(valueA), errorA => new Result<B>(errorA));
    public static Task<Result<B>> Select<A, B>(this Result<A> resultA, Func<A, Task<B>> selectTaskB) =>
        resultA.Match(async valueA => new Result<B>(await selectTaskB(valueA)), errorA => Task.FromResult(new Result<B>(errorA)));
    public static Task<Result<B>> Select<A, B>(this Result<A> resultA, Func<A, Task<Result<B>>> selectTaskResultB) =>
        resultA.Match(valueA => selectTaskResultB(valueA), errorA => Task.FromResult(new Result<B>(errorA)));

    public static async Task<Result<B>> Select<A, B>(this Task<Result<A>> taskResultA, Func<A, B> selectB) =>
        (await taskResultA).Select(selectB);
    public static async Task<Result<B>> Select<A, B>(this Task<Result<A>> taskResultA, Func<A, Result<B>> selectResultB) =>
        (await taskResultA).Select(selectResultB);
    public static async Task<Result<B>> Select<A, B>(this Task<Result<A>> taskResultA, Func<A, Task<B>> selectTaskB) =>
        await (await taskResultA).Select(selectTaskB);
    public static async Task<Result<B>> Select<A, B>(this Task<Result<A>> taskResultA, Func<A, Task<Result<B>>> selectTaskResultB) =>
        await (await taskResultA).Select(selectTaskResultB);

    public static Result<C> SelectMany<A, B, C>(this Result<A> resultA, Func<A, Result<B>> selectResultB, Func<A, B, C> selectC) =>
        resultA.Select(a => selectResultB(a).Select(b => selectC(a, b)));
    public static Result<C> SelectMany<A, B, C>(this Result<A> resultA, Func<A, Result<B>> selectResultB, Func<A, B, Result<C>> selectResultC) =>
        resultA.Select(a => selectResultB(a).Select(b => selectResultC(a, b)));
    public static async Task<Result<C>> SelectMany<A, B, C>(this Result<A> resultA, Func<A, Result<B>> selectResultB, Func<A, B, Task<C>> selectTaskC) =>
        await resultA.Select(a => selectResultB(a).Select(b => selectTaskC(a, b)));
    public static async Task<Result<C>> SelectMany<A, B, C>(this Result<A> resultA, Func<A, Result<B>> selectResultB, Func<A, B, Task<Result<C>>> selectTaskResultC) =>
        await resultA.Select(a => selectResultB(a).Select(b => selectTaskResultC(a, b)));

    public static async Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, Result<B>> selectResultB, Func<A, B, C> selectC) =>
        (await taskResultA).SelectMany(selectResultB, selectC);
    public static async Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, Result<B>> selectResultB, Func<A, B, Result<C>> selectResultC) =>
        (await taskResultA).SelectMany(selectResultB, selectResultC);
    public static async Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, Result<B>> selectResultB, Func<A, B, Task<C>> selectTaskC) =>
        await (await taskResultA).SelectMany(selectResultB, selectTaskC);
    public static async Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, Result<B>> selectResultB, Func<A, B, Task<Result<C>>> selectTaskResultC) =>
        await (await taskResultA).SelectMany(selectResultB, selectTaskResultC);

    public static async Task<Result<C>> SelectMany<A, B, C>(this Result<A> resultA, Func<A, Task<Result<B>>> selectTaskResultB, Func<A, B, C> selectC) =>
        await resultA.Select(async a => (await selectTaskResultB(a)).Select(b => selectC(a, b)));
    public static async Task<Result<C>> SelectMany<A, B, C>(this Result<A> resultA, Func<A, Task<Result<B>>> selectTaskResultB, Func<A, B, Result<C>> selectResultC) =>
        await resultA.Select(async a => (await selectTaskResultB(a)).Select(b => selectResultC(a, b)));
    public static async Task<Result<C>> SelectMany<A, B, C>(this Result<A> resultA, Func<A, Task<Result<B>>> selectTaskResultB, Func<A, B, Task<C>> selectTaskC) =>
        await resultA.Select(async a => await (await selectTaskResultB(a)).Select(async b => await selectTaskC(a, b)));
    public static async Task<Result<C>> SelectMany<A, B, C>(this Result<A> resultA, Func<A, Task<Result<B>>> selectTaskResultB, Func<A, B, Task<Result<C>>> selectTaskResultC) =>
        await resultA.Select(async a => await (await selectTaskResultB(a)).Select(async b => await selectTaskResultC(a, b)));

    public static async Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, Task<Result<B>>> selectTaskResultB, Func<A, B, C> selectC) =>
        await (await taskResultA).SelectMany(selectTaskResultB, selectC);
    public static async Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, Task<Result<B>>> selectTaskResultB, Func<A, B, Result<C>> selectResultC) =>
        await (await taskResultA).SelectMany(selectTaskResultB, selectResultC);
    public static async Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, Task<Result<B>>> selectTaskResultB, Func<A, B, Task<C>> selectTaskC) =>
        await (await taskResultA).SelectMany(selectTaskResultB, selectTaskC);
    public static async Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, Task<Result<B>>> selectTaskResultB, Func<A, B, Task<Result<C>>> selectTaskResultC) =>
        await (await taskResultA).SelectMany(selectTaskResultB, selectTaskResultC);

    #endregion
}