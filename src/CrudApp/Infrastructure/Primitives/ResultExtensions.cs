using System.Diagnostics.CodeAnalysis;

namespace CrudApp.Infrastructure.Primitives;

public static class ResultExtensions
{
    public static Result<T> ToResult<T>(this T value) where T : notnull => new(value);
    public static Task<Result<T>> ToResult<T>(this Task<T> taskValue) where T : notnull => taskValue.Select(value => value.ToResult());
    public static Result<T> ToResult<T>(this Error error) where T : notnull => new(error);
    public static Task<Result<T>> ToResult<T>(this Task<Error> taskError) where T : notnull => taskError.Select(error => error.ToResult<T>());

    

    public static bool TryGetValue<T>(this Result<T> result, [NotNullWhen(true)] out T? value, [NotNullWhen(false)] out Error? error) where T : notnull
    {
        (var hasValue, value, error) = result.Match(v => (true, v, default(Error?)), e => (false, default(T?), e));
        return hasValue;
    }
    public static bool TryGetValue<T>(this Result<T> result, [NotNullWhen(true)] out T? value) where T : notnull =>
        result.TryGetValue(out value, out var _);
    public static bool TryGetError<T>(this Result<T> result, [NotNullWhen(true)] out Error? error, [NotNullWhen(false)] out T? value) where T : notnull =>
        !result.TryGetValue(out value, out error);
    public static bool TryGetError<T>(this Result<T> result, [NotNullWhen(true)] out Error? error) where T : notnull =>
        result.TryGetError(out error, out var _);


    public static Result<T> Use<T>(this Result<T> result, Action<T> func) where T : notnull =>
        result.Select(v => { func(v); return v; });
    public static Task<Result<T>> Use<T>(this Result<T> result, Func<T, Task> func) where T : notnull =>
        result.Select(v => func(v).Select(() => v));
    public static Task<Result<T>> Use<T>(this Task<Result<T>> taskResult, Action<T> func) where T : notnull =>
        taskResult.Select(result => result.Use(func));
    public static Task<Result<T>> Use<T>(this Task<Result<T>> taskResult, Func<T, Task> func) where T : notnull =>
        taskResult.Select(result => result.Use(func));

    // from a in Result<A> select Action<A>
    public static Result<Nothing> Select<T>(this Result<T> result, Action<T> func) where T : notnull =>
        result.Select(v => { func(v); return Nothing.Instance.ToResult(); });
    // from a in Result<A> select Func<A, Task>
    public static Task<Result<Nothing>> Select<T>(this Result<T> result, Func<T, Task> func) where T : notnull =>
        result.Select(v => func(v).Select(() => Result.Nothing));
    // from a in Result<A> select Func<A, B>
    public static Result<B> Select<A, B>(this Result<A> resultA, Func<A, B> selectB) where A : notnull where B : notnull =>
        resultA.Match(valueA => new Result<B>(selectB(valueA)), errorA => new Result<B>(errorA));
    // from a in Result<A> select Func<A, Result<B>>
    public static Result<B> Select<A, B>(this Result<A> resultA, Func<A, Result<B>> selectResultB) where A : notnull where B : notnull =>
        resultA.Match(valueA => selectResultB(valueA), errorA => new Result<B>(errorA));
    // from a in Result<A> select Func<A, Task<B>>
    public static Task<Result<B>> Select<A, B>(this Result<A> resultA, Func<A, Task<B>> selectTaskB) where A : notnull where B : notnull =>
        resultA.Match(valueA => selectTaskB(valueA).Select(valueB => valueB.ToResult()), errorA => Task.FromResult(new Result<B>(errorA)));
    // from a in Result<A> select Func<A, Task<Result<B>>>
    public static Task<Result<B>> Select<A, B>(this Result<A> resultA, Func<A, Task<Result<B>>> selectTaskResultB) where A : notnull where B : notnull =>
        resultA.Match(valueA => selectTaskResultB(valueA), errorA => Task.FromResult(new Result<B>(errorA)));


    // from a in Task<Result<A>> select Action<A>
    public static Task<Result<Nothing>> Select<T>(this Task<Result<T>> taskResult, Action<T> func) where T : notnull =>
        taskResult.Select(result => result.Select(func));
    // from a in Task<Result<A>> select Func<A, Task>
    public static Task<Result<Nothing>> Select<T>(this Task<Result<T>> taskResult, Func<T, Task> func) where T : notnull =>
        taskResult.Select(result => result.Select(func));
    // from a in Task<Result<A>> select Func<A, B>
    public static Task<Result<B>> Select<A, B>(this Task<Result<A>> taskResultA, Func<A, B> selectB) where A : notnull where B : notnull =>
        taskResultA.Select(resultA => resultA.Select(selectB));
    // from a in Task<Result<A>> select Func<A, Result<B>>
    public static Task<Result<B>> Select<A, B>(this Task<Result<A>> taskResultA, Func<A, Result<B>> selectResultB) where A : notnull where B : notnull =>
        taskResultA.Select(resultA => resultA.Select(selectResultB));
    // from a in Task<Result<A>> select Func<A, Task<B>>
    public static Task<Result<B>> Select<A, B>(this Task<Result<A>> taskResultA, Func<A, Task<B>> selectTaskB) where A : notnull where B : notnull =>
        taskResultA.Select(resultA => resultA.Select(selectTaskB));
    // from a in Task<Result<A>> select Func<A, Task<Result<B>>>
    public static Task<Result<B>> Select<A, B>(this Task<Result<A>> taskResultA, Func<A, Task<Result<B>>> selectTaskResultB) where A : notnull where B : notnull =>
        taskResultA.Select(resultA => resultA.Select(selectTaskResultB));



    // from a in Task<Result<A>> from b in Task<Result<B>> select Func<A, B, C>
    public static Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, Task<Result<B>>> selectTaskResultB, Func<A, B, C> selectC) where A : notnull where B : notnull where C : notnull =>
        taskResultA.Select(resultA => resultA.Select(a => selectTaskResultB(a).Select(b => selectC(a, b))));
    // from a in Task<Result<A>> from b in Task<Result<B>> select Func<A, B, Result<C>>
    public static Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, Task<Result<B>>> selectTaskResultB, Func<A, B, Result<C>> selectResultC) where A : notnull where B : notnull where C : notnull =>
        taskResultA.Select(resultA => resultA.Select(a => selectTaskResultB(a).Select(b => selectResultC(a, b))));
    // from a in Task<Result<A>> from b in Task<Result<B>> select Func<A, B, Task<C>>
    public static Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, Task<Result<B>>> selectTaskResultB, Func<A, B, Task<C>> selectTaskC) where A : notnull where B : notnull where C : notnull =>
        taskResultA.Select(resultA => resultA.Select(a => selectTaskResultB(a).Select(b => selectTaskC(a, b))));
    // from a in Task<Result<A>> from b in Task<Result<B>> select Func<A, B, Task<Result<C>>>
    public static Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, Task<Result<B>>> selectTaskResultB, Func<A, B, Task<Result<C>>> selectTaskResultC) where A : notnull where B : notnull where C : notnull =>
        taskResultA.Select(resultA => resultA.Select(a => selectTaskResultB(a).Select(b => selectTaskResultC(a, b))));

    //
    // Old attempt as Select and SelectMany
    //


    //// from a in Result<A> select Func<A, B>
    //public static Result<B> Select<A, B>(this Result<A> resultA, Func<A, B> selectB) where A : notnull where B : notnull =>
    //    resultA.Match(valueA => new Result<B>(selectB(valueA)), errorA => new Result<B>(errorA));
    //// from a in Result<A> select Func<A, Result<B>>
    //public static Result<B> Select<A, B>(this Result<A> resultA, Func<A, Result<B>> selectResultB) where A : notnull where B : notnull =>
    //    resultA.Match(valueA => selectResultB(valueA), errorA => new Result<B>(errorA));
    //// from a in Result<A> select Func<A, Task<B>>
    //public static Task<Result<B>> Select<A, B>(this Result<A> resultA, Func<A, Task<B>> selectTaskB) where A : notnull where B : notnull =>
    //    resultA.Match(valueA => selectTaskB(valueA).Select(valueB => valueB.ToResult()), errorA => Task.FromResult(new Result<B>(errorA)));
    //// from a in Result<A> select Func<A, Task<Result<B>>>
    //public static Task<Result<B>> Select<A, B>(this Result<A> resultA, Func<A, Task<Result<B>>> selectTaskResultB) where A : notnull where B : notnull =>
    //    resultA.Match(valueA => selectTaskResultB(valueA), errorA => Task.FromResult(new Result<B>(errorA)));


    //// from a in Task<Result<A>> select Func<A, B>
    //public static Task<Result<B>> Select<A, B>(this Task<Result<A>> taskResultA, Func<A, B> selectB) where A : notnull where B : notnull =>
    //    taskResultA.Select(resultA => resultA.Select(selectB));
    //// from a in Task<Result<A>> select Func<A, Result<B>>
    //public static Task<Result<B>> Select<A, B>(this Task<Result<A>> taskResultA, Func<A, Result<B>> selectResultB) where A : notnull where B : notnull =>
    //    taskResultA.Select(resultA => resultA.Select(selectResultB));
    //// from a in Task<Result<A>> select Func<A, Task<B>>
    //public static Task<Result<B>> Select<A, B>(this Task<Result<A>> taskResultA, Func<A, Task<B>> selectTaskB) where A : notnull where B : notnull =>
    //    taskResultA.Select(resultA => resultA.Select(selectTaskB));
    //// from a in Task<Result<A>> select Func<A, Task<Result<B>>>
    //public static Task<Result<B>> Select<A, B>(this Task<Result<A>> taskResultA, Func<A, Task<Result<B>>> selectTaskResultB) where A : notnull where B : notnull =>
    //    taskResultA.Select(resultA => resultA.Select(selectTaskResultB));





    //// from a in Result<A> from b in B select Func<A, B, C>
    //public static Result<C> SelectMany<A, B, C>(this Result<A> resultA, Func<A, B> selectB, Func<A, B, C> selectC) where A : notnull where B : notnull where C : notnull =>
    //    resultA.Select(a => selectC(a, selectB(a)));
    //// from a in Result<A> from b in B select Func<A, B, Result<C>>
    //public static Result<C> SelectMany<A, B, C>(this Result<A> resultA, Func<A, B> selectB, Func<A, B, Result<C>> selectResultC) where A : notnull where B : notnull where C : notnull =>
    //    resultA.Select(a => selectResultC(a, selectB(a)));
    //// from a in Result<A> from b in B select Func<A, B, Task<C>>
    //public static Task<Result<C>> SelectMany<A, B, C>(this Result<A> resultA, Func<A, B> selectB, Func<A, B, Task<C>> selectTaskC) where A : notnull where B : notnull where C : notnull =>
    //    resultA.Select(a => selectTaskC(a, selectB(a)));
    //// from a in Result<A> from b in B select Func<A, B, Task<Result<C>>>
    //public static Task<Result<C>> SelectMany<A, B, C>(this Result<A> resultA, Func<A, B> selectB, Func<A, B, Task<Result<C>>> selectTaskResultC) where A : notnull where B : notnull where C : notnull =>
    //    resultA.Select(a => selectTaskResultC(a, selectB(a)));


    //// from a in Task<Result<A>> from b in B select Func<A, B, C>
    //public static Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, B> selectB, Func<A, B, C> selectC) where A : notnull where B : notnull where C : notnull =>
    //    taskResultA.Select(resultA => resultA.SelectMany(selectB, selectC));
    //// from a in Task<Result<A>> from b in B select Func<A, B, Result<C>>
    //public static Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, B> selectB, Func<A, B, Result<C>> selectResultC) where A : notnull where B : notnull where C : notnull =>
    //    taskResultA.Select(resultA => resultA.SelectMany(selectB, selectResultC));
    //// from a in Task<Result<A>> from b in B select Func<A, B, Task<C>>
    //public static Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, B> selectB, Func<A, B, Task<C>> selectTaskC) where A : notnull where B : notnull where C : notnull =>
    //    taskResultA.Select(resultA => resultA.SelectMany(selectB, selectTaskC));
    //// from a in Task<Result<A>> from b in B select Func<A, B, Task<Result<C>>>
    //public static Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, B> selectB, Func<A, B, Task<Result<C>>> selectTaskResultC) where A : notnull where B : notnull where C : notnull =>
    //    taskResultA.Select(resultA => resultA.SelectMany(selectB, selectTaskResultC));






    //// from a in Result<A> from b in Result<B> select Func<A, B, C>
    //public static Result<C> SelectMany<A, B, C>(this Result<A> resultA, Func<A, Result<B>> selectResultB, Func<A, B, C> selectC) where A : notnull where B : notnull where C : notnull =>
    //    resultA.Select(a => selectResultB(a).Select(b => selectC(a, b)));
    //// from a in Result<A> from b in Result<B> select Func<A, B, Result<C>>
    //public static Result<C> SelectMany<A, B, C>(this Result<A> resultA, Func<A, Result<B>> selectResultB, Func<A, B, Result<C>> selectResultC) where A : notnull where B : notnull where C : notnull =>
    //    resultA.Select(a => selectResultB(a).Select(b => selectResultC(a, b)));
    //// from a in Result<A> from b in Result<B> select Func<A, B, Task<C>>
    //public static Task<Result<C>> SelectMany<A, B, C>(this Result<A> resultA, Func<A, Result<B>> selectResultB, Func<A, B, Task<C>> selectTaskC) where A : notnull where B : notnull where C : notnull =>
    //    resultA.Select(a => selectResultB(a).Select(b => selectTaskC(a, b)));
    //// from a in Result<A> from b in Result<B> select Func<A, B, Task<Result<C>>>
    //public static Task<Result<C>> SelectMany<A, B, C>(this Result<A> resultA, Func<A, Result<B>> selectResultB, Func<A, B, Task<Result<C>>> selectTaskResultC) where A : notnull where B : notnull where C : notnull =>
    //    resultA.Select(a => selectResultB(a).Select(b => selectTaskResultC(a, b)));


    //// from a in Task<Result<A>> from b in Result<B> select Func<A, B, C>
    //public static Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, Result<B>> selectResultB, Func<A, B, C> selectC) where A : notnull where B : notnull where C : notnull =>
    //    taskResultA.Select(resultA => resultA.SelectMany(selectResultB, selectC));
    //// from a in Task<Result<A>> from b in Result<B> select Func<A, B, Result<C>>
    //public static Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, Result<B>> selectResultB, Func<A, B, Result<C>> selectResultC) where A : notnull where B : notnull where C : notnull =>
    //    taskResultA.Select(resultA => resultA.SelectMany(selectResultB, selectResultC));
    //// from a in Task<Result<A>> from b in Result<B> select Func<A, B, Task<C>>
    //public static Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, Result<B>> selectResultB, Func<A, B, Task<C>> selectTaskC) where A : notnull where B : notnull where C : notnull =>
    //    taskResultA.Select(resultA => resultA.SelectMany(selectResultB, selectTaskC));
    //// from a in Task<Result<A>> from b in Result<B> select Func<A, B, Task<Result<C>>>
    //public static Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, Result<B>> selectResultB, Func<A, B, Task<Result<C>>> selectTaskResultC) where A : notnull where B : notnull where C : notnull =>
    //    taskResultA.Select(resultA => resultA.SelectMany(selectResultB, selectTaskResultC));






    //// from a in Result<A> from b in Task<B> select Func<A, B, C>
    //public static Task<Result<C>> SelectMany<A, B, C>(this Result<A> resultA, Func<A, Task<B>> selectTaskB, Func<A, B, C> selectC) where A : notnull where B : notnull where C : notnull =>
    //    resultA.Select(a => selectTaskB(a).Select(b => selectC(a, b)));
    //// from a in Result<A> from b in Task<B> select Func<A, B, Result<C>>
    //public static Task<Result<C>> SelectMany<A, B, C>(this Result<A> resultA, Func<A, Task<B>> selectTaskB, Func<A, B, Result<C>> selectResultC) where A : notnull where B : notnull where C : notnull =>
    //    resultA.Select(a => selectTaskB(a).Select(b => selectResultC(a, b)));
    //// from a in Result<A> from b in Task<B> select Func<A, B, Task<C>>
    //public static Task<Result<C>> SelectMany<A, B, C>(this Result<A> resultA, Func<A, Task<B>> selectTaskB, Func<A, B, Task<C>> selectTaskC) where A : notnull where B : notnull where C : notnull =>
    //    resultA.Select(a => selectTaskB(a).Select(b => selectTaskC(a, b)));
    //// from a in Result<A> from b in Task<B> select Func<A, B, Task<Result<C>>>
    //public static Task<Result<C>> SelectMany<A, B, C>(this Result<A> resultA, Func<A, Task<B>> selectTaskB, Func<A, B, Task<Result<C>>> selectTaskResultC) where A : notnull where B : notnull where C : notnull =>
    //    resultA.Select(a => selectTaskB(a).Select(b => selectTaskResultC(a, b)));


    //// from a in Task<Result<A>> from b in Task<B> select Func<A, B, C>
    //public static Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, Task<B>> selectTaskB, Func<A, B, C> selectC) where A : notnull where B : notnull where C : notnull =>
    //    taskResultA.Select(resultA => resultA.SelectMany(selectTaskB, selectC));
    //// from a in Task<Result<A>> from b in Task<B> select Func<A, B, Result<C>>
    //public static Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, Task<B>> selectTaskB, Func<A, B, Result<C>> selectResultC) where A : notnull where B : notnull where C : notnull =>
    //    taskResultA.Select(resultA => resultA.SelectMany(selectTaskB, selectResultC));
    //// from a in Task<Result<A>> from b in Task<B> select Func<A, B, Task<C>>
    //public static Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, Task<B>> selectTaskB, Func<A, B, Task<C>> selectTaskC) where A : notnull where B : notnull where C : notnull =>
    //    taskResultA.Select(resultA => resultA.SelectMany(selectTaskB, selectTaskC));
    //// from a in Task<Result<A>> from b in Task<B> select Func<A, B, Task<Result<C>>>
    //public static Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, Task<B>> selectTaskB, Func<A, B, Task<Result<C>>> selectTaskResultC) where A : notnull where B : notnull where C : notnull =>
    //    taskResultA.Select(resultA => resultA.SelectMany(selectTaskB, selectTaskResultC));






    //// from a in Result<A> from b in Task<Result<B>> select Func<A, B, C>
    //public static Task<Result<C>> SelectMany<A, B, C>(this Result<A> resultA, Func<A, Task<Result<B>>> selectTaskResultB, Func<A, B, C> selectC) where A : notnull where B : notnull where C : notnull =>
    //    resultA.Select(a => selectTaskResultB(a).Select(b => selectC(a, b)));
    //// from a in Result<A> from b in Task<Result<B>> select Func<A, B, Result<C>>
    //public static Task<Result<C>> SelectMany<A, B, C>(this Result<A> resultA, Func<A, Task<Result<B>>> selectTaskResultB, Func<A, B, Result<C>> selectResultC) where A : notnull where B : notnull where C : notnull =>
    //    resultA.Select(a => selectTaskResultB(a).Select(b => selectResultC(a, b)));
    //// from a in Result<A> from b in Task<Result<B>> select Func<A, B, Task<C>>
    //public static Task<Result<C>> SelectMany<A, B, C>(this Result<A> resultA, Func<A, Task<Result<B>>> selectTaskResultB, Func<A, B, Task<C>> selectTaskC) where A : notnull where B : notnull where C : notnull =>
    //    resultA.Select(a => selectTaskResultB(a).Select(b => selectTaskC(a, b)));
    //// from a in Result<A> from b in Task<Result<B>> select Func<A, B, Task<Result<C>>>
    //public static Task<Result<C>> SelectMany<A, B, C>(this Result<A> resultA, Func<A, Task<Result<B>>> selectTaskResultB, Func<A, B, Task<Result<C>>> selectTaskResultC) where A : notnull where B : notnull where C : notnull =>
    //    resultA.Select(a => selectTaskResultB(a).Select(b => selectTaskResultC(a, b)));



    //// from a in Task<Result<A>> from b in Task<Result<B>> select Func<A, B, C>
    //public static Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, Task<Result<B>>> selectTaskResultB, Func<A, B, C> selectC) where A : notnull where B : notnull where C : notnull =>
    //    taskResultA.Select(resultA => resultA.SelectMany(selectTaskResultB, selectC));
    //// from a in Task<Result<A>> from b in Task<Result<B>> select Func<A, B, Result<C>>
    //public static Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, Task<Result<B>>> selectTaskResultB, Func<A, B, Result<C>> selectResultC) where A : notnull where B : notnull where C : notnull =>
    //    taskResultA.Select(resultA => resultA.SelectMany(selectTaskResultB, selectResultC));
    //// from a in Task<Result<A>> from b in Task<Result<B>> select Func<A, B, Task<C>>
    //public static Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, Task<Result<B>>> selectTaskResultB, Func<A, B, Task<C>> selectTaskC) where A : notnull where B : notnull where C : notnull =>
    //    taskResultA.Select(resultA => resultA.SelectMany(selectTaskResultB, selectTaskC));
    //// from a in Task<Result<A>> from b in Task<Result<B>> select Func<A, B, Task<Result<C>>>
    //public static Task<Result<C>> SelectMany<A, B, C>(this Task<Result<A>> taskResultA, Func<A, Task<Result<B>>> selectTaskResultB, Func<A, B, Task<Result<C>>> selectTaskResultC) where A : notnull where B : notnull where C : notnull =>
    //    taskResultA.Select(resultA => resultA.SelectMany(selectTaskResultB, selectTaskResultC));

}
