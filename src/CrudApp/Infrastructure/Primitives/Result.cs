using System.Collections;

namespace CrudApp.Infrastructure.Primitives;


public static class Result
{
    public static Result<Nothing> Nothing { get; } = new Result<Nothing>(Primitives.Nothing.Instance);
}

/// <summary>
/// When a <see cref="Result{T}"/> gets returned from a controller action,
/// it is the inner value or the inner error (converted to <see cref="ProblemDetails"/>) that gets returned to the client.
/// </summary>
/// <typeparam name="T"></typeparam>
public readonly struct Result<T> : IInfrastructureResult where T : notnull
{
    private readonly Maybe<Error> _maybeError;
    private readonly Maybe<T> _maybeValue;

    public Result()
    {
        // This type should never be created without a value or an error.
        // Note that this constructor does not get called when a Result<T> is created using the 'default' keyword.
        throw NewNotInitializedException();
    }
    public Result(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        _maybeError = error;
    }

    public Result(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        _maybeValue = value;
    }

    public static implicit operator Result<T>(T value) => new(value);
    public static implicit operator Result<T>(Error error) => new(error);
    public static implicit operator Result<Nothing>(Result<T> result) => result.Match(_ => Nothing.Instance.ToResult(), error => error);

    public R Match<R>(Func<T, R> ifHasValue, Func<Error, R> ifHasError)
    {
        var maybeError = _maybeError;
        return _maybeValue.Match(ifHasValue, 
            () => maybeError.Match(ifHasError,
            () => throw NewNotInitializedException()));
    }

    private static InvalidOperationException NewNotInitializedException() =>
        new($"{nameof(Result)} has not been initialized with a value or an error.");

    object IInfrastructureResult.GetValueOrError() => Match<object>(v => v, e => e);
}

/// <summary>
/// This interface is only supposed to be used by the infrastructure to avoid some reflection. Use <see cref="Result{T}"/> in you normal code.
/// </summary>
public interface IInfrastructureResult
{
    object GetValueOrError();
}