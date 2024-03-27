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
    private readonly bool _hasError;
    private readonly Error? _error;
    private readonly bool _hasValue;
    private readonly T? _value;

    [MemberNotNullWhen(true, nameof(_error))]
    [MemberNotNullWhen(false, nameof(_value))]
    private bool HasError { get { AssertHasValueOrError(); return _hasError; } }

    [MemberNotNullWhen(true, nameof(_value))]
    [MemberNotNullWhen(false, nameof(_error))]
    private bool HasValue { get { AssertHasValueOrError(); return _hasValue; } }

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

    public R Match<R>(Func<T, R> ifHasValue, Func<Error, R> ifHasError)
    {
        if (HasValue)
            return ifHasValue(_value);
        return ifHasError(_error);
    }

    public Result()
    {
        AssertHasValueOrError();
    }
    public Result(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        _error = error;
        _hasError = true;
    }

    public Result(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        _value = value;
        _hasValue = true;
    }

    public static implicit operator Result<T>(T value) => new(value);
    public static implicit operator Result<T>(Error error) => new(error);
    public static implicit operator Result<Nothing>(Result<T> result) => result.Match(_ => Nothing.Instance.ToResult(), error => error);

    private void AssertHasValueOrError()
    {
        if (!_hasError && !_hasValue)
            throw new InvalidOperationException($"{nameof(Result)} has not been initialized with a value or an error.");
    }
}

public static class Result
{
    public static Result<Nothing> Nothing { get; } = new Result<Nothing>(Primitives.Nothing.Instance);
    public static Result<T> ToResult<T>(this T value) where T : notnull => new(value);
    public static Result<T> ToResult<T>(this Error error) where T : notnull => new(error);


    public static Result<T> Use<T>(this Result<T> result, Action<T> func) where T : notnull =>
        result.Select(v => { func(v); return v; });
    public static async Task<Result<T>> Use<T>(this Result<T> result, Func<T, Task> func) where T : notnull =>
        await result.Select(async v => { await func(v); return v; });
    public static async Task<Result<T>> Use<T>(this Task<Result<T>> result, Action<T> func) where T : notnull =>
        (await result).Select(v => { func(v); return v; });
    public static async Task<Result<T>> Use<T>(this Task<Result<T>> result, Func<T, Task> func) where T : notnull =>
        await (await result).Select(async v => { await func(v); return v; });


    public static Result<Nothing> Select<T>(this Result<T> result, Action<T> func) where T : notnull =>
        result.Select(v => { func(v); return Nothing; });
    public static async Task<Result<Nothing>> Select<T>(this Result<T> result, Func<T, Task> func) where T : notnull =>
        await result.Select(async v => { await func(v); return Nothing; });
    public static async Task<Result<Nothing>> Select<T>(this Task<Result<T>> result, Action<T> func) where T : notnull =>
        (await result).Select(v => { func(v); return Nothing; });
    public static async Task<Result<Nothing>> Select<T>(this Task<Result<T>> result, Func<T, Task> func) where T : notnull =>
        await (await result).Select(async v => { await func(v); return Nothing; });

    public static Result<B> Select<A, B>(this Result<A> resultA, Func<A, B> selectB) where A : notnull where B : notnull =>
        resultA.Match(valueA => new Result<B>(selectB(valueA)), errorA => new Result<B>(errorA));
    public static Result<B> Select<A, B>(this Result<A> resultA, Func<A, Result<B>> selectResultB) where A : notnull where B : notnull =>
        resultA.Match(valueA => selectResultB(valueA), errorA => new Result<B>(errorA));
    public static Task<Result<B>> Select<A, B>(this Result<A> resultA, Func<A, Task<B>> selectTaskB) where A : notnull where B : notnull =>
        resultA.Match(async valueA => new Result<B>(await selectTaskB(valueA)), errorA => Task.FromResult(new Result<B>(errorA)));
    public static Task<Result<B>> Select<A, B>(this Result<A> resultA, Func<A, Task<Result<B>>> selectTaskResultB) where A : notnull where B : notnull =>
        resultA.Match(valueA => selectTaskResultB(valueA), errorA => Task.FromResult(new Result<B>(errorA)));

    public static async Task<Result<B>> Select<A, B>(this Task<Result<A>> taskResultA, Func<A, B> selectB) where A : notnull where B : notnull =>
        (await taskResultA).Select(selectB);
    public static async Task<Result<B>> Select<A, B>(this Task<Result<A>> taskResultA, Func<A, Result<B>> selectResultB) where A : notnull where B : notnull =>
        (await taskResultA).Select(selectResultB);
    public static async Task<Result<B>> Select<A, B>(this Task<Result<A>> taskResultA, Func<A, Task<B>> selectTaskB) where A : notnull where B : notnull =>
        await (await taskResultA).Select(selectTaskB);
    public static async Task<Result<B>> Select<A, B>(this Task<Result<A>> taskResultA, Func<A, Task<Result<B>>> selectTaskResultB) where A : notnull where B : notnull =>
        await (await taskResultA).Select(selectTaskResultB);

    public static Result<C> SelectMany<A, B, C>(this Result<A> resultA, Func<A, Result<B>> selectResultB, Func<A, B, C> selectC) where A : notnull where B : notnull where C : notnull =>
        resultA.Select(a => selectResultB(a).Select(b => selectC(a, b)));
    public static Result<C> SelectMany<A, B, C>(this Result<A> resultA, Func<A, Result<B>> selectResultB, Func<A, B, Result<C>> selectResultC) where A : notnull where B : notnull where C : notnull =>
        resultA.Select(a => selectResultB(a).Select(b => selectResultC(a, b)));
    public static Task<Result<C>> SelectMany<A, B, C>(this Result<A> resultA, Func<A, Result<B>> selectResultB, Func<A, B, Task<C>> selectTaskC) where A : notnull where B : notnull where C : notnull =>
        resultA.Select(a => selectResultB(a).Select(b => selectTaskC(a, b)));
    public static Task<Result<C>> SelectMany<A, B, C>(this Result<A> resultA, Func<A, Result<B>> selectResultB, Func<A, B, Task<Result<C>>> selectTaskResultC) where A : notnull where B : notnull where C : notnull =>
        resultA.Select(a => selectResultB(a).Select(b => selectTaskResultC(a, b)));

    public static async Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, Result<B>> selectResultB, Func<A, B, C> selectC) where A : notnull where B : notnull where C : notnull =>
        (await taskResultA).SelectMany(selectResultB, selectC);
    public static async Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, Result<B>> selectResultB, Func<A, B, Result<C>> selectResultC) where A : notnull where B : notnull where C : notnull =>
        (await taskResultA).SelectMany(selectResultB, selectResultC);
    public static async Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, Result<B>> selectResultB, Func<A, B, Task<C>> selectTaskC) where A : notnull where B : notnull where C : notnull =>
        await (await taskResultA).SelectMany(selectResultB, selectTaskC);
    public static async Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, Result<B>> selectResultB, Func<A, B, Task<Result<C>>> selectTaskResultC) where A : notnull where B : notnull where C : notnull =>
        await (await taskResultA).SelectMany(selectResultB, selectTaskResultC);

    public static Task<Result<C>> SelectMany<A, B, C>(this Result<A> resultA, Func<A, Task<Result<B>>> selectTaskResultB, Func<A, B, C> selectC) where A : notnull where B : notnull where C : notnull =>
        resultA.Select(a => selectTaskResultB(a).Select(b => selectC(a, b)));
    public static Task<Result<C>> SelectMany<A, B, C>(this Result<A> resultA, Func<A, Task<Result<B>>> selectTaskResultB, Func<A, B, Result<C>> selectResultC) where A : notnull where B : notnull where C : notnull =>
        resultA.Select(a => selectTaskResultB(a).Select(b => selectResultC(a, b)));
    public static Task<Result<C>> SelectMany<A, B, C>(this Result<A> resultA, Func<A, Task<Result<B>>> selectTaskResultB, Func<A, B, Task<C>> selectTaskC) where A : notnull where B : notnull where C : notnull =>
        resultA.Select(a => selectTaskResultB(a).Select(b => selectTaskC(a, b)));
    public static Task<Result<C>> SelectMany<A, B, C>(this Result<A> resultA, Func<A, Task<Result<B>>> selectTaskResultB, Func<A, B, Task<Result<C>>> selectTaskResultC) where A : notnull where B : notnull where C : notnull =>
        resultA.Select(a => selectTaskResultB(a).Select(b => selectTaskResultC(a, b)));

    public static async Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, Task<Result<B>>> selectTaskResultB, Func<A, B, C> selectC) where A : notnull where B : notnull where C : notnull =>
        await (await taskResultA).SelectMany(selectTaskResultB, selectC);
    public static async Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, Task<Result<B>>> selectTaskResultB, Func<A, B, Result<C>> selectResultC) where A : notnull where B : notnull where C : notnull =>
        await (await taskResultA).SelectMany(selectTaskResultB, selectResultC);
    public static async Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, Task<Result<B>>> selectTaskResultB, Func<A, B, Task<C>> selectTaskC) where A : notnull where B : notnull where C : notnull =>
        await (await taskResultA).SelectMany(selectTaskResultB, selectTaskC);
    public static async Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, Task<Result<B>>> selectTaskResultB, Func<A, B, Task<Result<C>>> selectTaskResultC) where A : notnull where B : notnull where C : notnull =>
        await (await taskResultA).SelectMany(selectTaskResultB, selectTaskResultC);

}