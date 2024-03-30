namespace CrudApp.Infrastructure.Primitives;

public static class MaybeExtensions
{
    // The type constraints on T are not part of the signature.
    // So we can not have "ToMaybe<T>(this T? refValue) where T : class" and "ToMaybe<T>(this T structValue) where T : struct" in the same class.
    // So we make a class for methods with class constraint and another for methods with strut constraints.
    // The compile knows which variant of the methods to use.
}

public static class MaybeReferenceTypeExtensions
{
    public static Maybe<T> ToMaybe<T>(this T? refValue) where T : class => refValue is null ? Maybe.NoValue<T>() : new(refValue);

    public static Maybe<B> Select<A, B>(this Maybe<A> maybe, Func<A, B?> selectB) where A : notnull where B : class =>
        maybe.Match(a => selectB(a).ToMaybe(), () => Maybe.NoValue<B>());

    public static Task<Maybe<B>> Select<A, B>(this Maybe<A> maybe, Func<A, Task<B?>> selectTaskB) where A : notnull where B : class =>
        maybe.Match(a => selectTaskB(a).Select(b => b.ToMaybe()), () => Task.FromResult(Maybe.NoValue<B>()));

    public static Maybe<B> Select<A, B>(this A? nullable, Func<A, B?> selectB) where A : struct where B : class =>
       nullable.ToMaybe().Select(selectB);

    public static Task<Maybe<B>> Select<A, B>(this A? nullable, Func<A, Task<B?>> selectTaskB) where A : struct where B : class =>
        nullable.ToMaybe().Select(selectTaskB);
}

public static class MaybeValueTypeExtensions
{
    public static Maybe<T> ToMaybe<T>(this T structValue) where T : struct => new(structValue);
    public static Maybe<T> ToMaybe<T>(this T? nullableStructValue) where T : struct => nullableStructValue is null ? Maybe.NoValue<T>() : new(nullableStructValue.Value);


    public static Maybe<B> Select<A, B>(this Maybe<A> maybe, Func<A, B> selectB) where A : notnull where B : struct =>
        maybe.Match(a => selectB(a).ToMaybe(), () => Maybe.NoValue<B>());
    public static Maybe<B> Select<A, B>(this Maybe<A> maybe, Func<A, B?> selectNullableB) where A : notnull where B : struct =>
        maybe.Match(a => selectNullableB(a).ToMaybe(), () => Maybe.NoValue<B>());

    
    public static Task<Maybe<B>> Select<A, B>(this Maybe<A> maybe, Func<A, Task<B>> selectTaskB) where A : notnull where B : struct =>
        maybe.Match(a => selectTaskB(a).Select(b => b.ToMaybe()), () => Task.FromResult(Maybe.NoValue<B>()));
    public static Task<Maybe<B>> Select<A, B>(this Maybe<A> maybe, Func<A, Task<B?>> selectTaskNullableB) where A : notnull where B : struct =>
        maybe.Match(a => selectTaskNullableB(a).Select(b => b.ToMaybe()), () => Task.FromResult(Maybe.NoValue<B>()));

    public static Maybe<B> Select<A, B>(this A? nullable, Func<A, B> selectB) where A : struct where B : struct =>
        nullable.ToMaybe().Select(selectB);
    public static Maybe<B> Select<A, B>(this A? nullable, Func<A, B?> selectB) where A : struct where B : struct =>
        nullable.ToMaybe().Select(selectB);

    public static Task<Maybe<B>> Select<A, B>(this A? nullable, Func<A, Task<B>> selectTaskB) where A : struct where B : struct =>
        nullable.ToMaybe().Select(selectTaskB);
    public static Task<Maybe<B>> Select<A, B>(this A? nullable, Func<A, Task<B?>> selectTaskB) where A : struct where B : struct =>
        nullable.ToMaybe().Select(selectTaskB);
}
