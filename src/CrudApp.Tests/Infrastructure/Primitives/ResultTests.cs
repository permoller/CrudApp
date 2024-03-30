using CrudApp.Infrastructure.Testing;
using System.Diagnostics.CodeAnalysis;

namespace CrudApp.Tests.Infrastructure.Primitives;
public class ResultTests
{
    [Fact]
    public void Match()
    {
        var hasValue = false;
        var hasError = false;
        var result = 3.ToResult();
        result.Match(i => hasValue = true, e => hasError = true);
        Assert.True(hasValue);
        Assert.False(hasError);

        hasValue = false;
        hasError = false;
        result = new Error.EntityNotFound(typeof(InfrastructureTestEntity), 4).ToResult<int>();
        result.Match(i => hasValue = true, e => hasError = true);
        Assert.False(hasValue);
        Assert.True(hasError);
    }

    [Fact]
    [SuppressMessage("Minor Code Smell", "S1481:Unused local variables should be removed", Justification = "Required for testing implicit cast")]
    public void CreatingResultWithoutValueOrErrorShouldFail()
    {
        // Using default does not call the parameterless constructor, so we can't make it fail.
        // Instead we fail once we try to access the value.
        Result<object> result = default;
        Assert.Throws<InvalidOperationException>(() => result.Match(_ => 0, _ => 0));

        Error nullError = null!;
        InfrastructureTestEntity obj = null!;

        Assert.Throws<InvalidOperationException>(() => new Result<InfrastructureTestEntity>());
        Assert.Throws<InvalidOperationException>(() => new Result<int>());
        
        Assert.Throws<ArgumentNullException>(() => new Result<InfrastructureTestEntity>(obj));
        Assert.Throws<ArgumentNullException>(() => { Result<InfrastructureTestEntity> r = obj; });
        Assert.Throws<ArgumentNullException>(() => obj.ToResult());

        Assert.Throws<ArgumentNullException>(() => new Result<InfrastructureTestEntity>(nullError));
        Assert.Throws<ArgumentNullException>(() => new Result<int>(nullError));

        Assert.Throws<ArgumentNullException>(() => { Result<InfrastructureTestEntity> r = nullError; });
        Assert.Throws<ArgumentNullException>(() => { Result<int> r = nullError; });

        Assert.Throws<ArgumentNullException>(() => nullError.ToResult<InfrastructureTestEntity>());
        Assert.Throws<ArgumentNullException>(() => nullError.ToResult<int>());
    }

    [Fact]
    public void ImplicitCasts()
    {
        Result<int> result1 = 3;
        Assert.True(result1.TryGetValue(out var value, out var error));
        Assert.Equal(3, value);
        Assert.Null(error);

        Result<int> result2 = new Error.EntityNotFound(typeof(InfrastructureTestEntity), 4);
        Assert.False(result2.TryGetValue(out var value2, out var error2));
        Assert.Equal(0, value2);
        Assert.IsType<Error.EntityNotFound>(error2);
    }

    [Fact]
    public void ToResult()
    {
        var intValue = 3;
        Result<int> intResult = intValue.ToResult();
        Assert.True(intResult.TryGetValue(out var valueFromIntRestult, out var errorFromIntResult));
        Assert.Equal(intValue, valueFromIntRestult);
        Assert.Null(errorFromIntResult);

        var objValue = new InfrastructureTestEntity(new());
        Result<InfrastructureTestEntity> objResult = objValue.ToResult();
        Assert.True(objResult.TryGetValue(out var valueFromObjResult, out var errorFromObjResult));
        Assert.Equal(objValue, valueFromObjResult);
        Assert.Null(errorFromObjResult);

        var error = new Error.EntityNotFound(typeof(InfrastructureTestEntity), 4);
        Result<InfrastructureTestEntity> errorResult = error.ToResult<InfrastructureTestEntity>();
        Assert.False(errorResult.TryGetValue(out var valueFromErrorResult, out var errorFromErrorResult));
        Assert.Null(valueFromErrorResult);
        Assert.Equal(error, errorFromErrorResult);
    }

    
    [Fact]
    public void TryGetValue()
    {
        var expectedValue = 3;
        var expectedError = new Error.EntityNotFound(typeof(InfrastructureTestEntity), 4);
        var resultWithValue = expectedValue.ToResult();
        var resultWithError = expectedError.ToResult<int>();

        Assert.True(resultWithValue.TryGetValue(out var value, out var error));
        Assert.Equal(expectedValue, value);
        Assert.Null(error);

        Assert.True(resultWithValue.TryGetValue(out value));
        Assert.Equal(expectedValue, value);


        Assert.False(resultWithError.TryGetValue(out value, out error));
        Assert.Equal(default, value);
        Assert.Equal(expectedError, error);

        Assert.False(resultWithError.TryGetValue(out value));
        Assert.Equal(default, value);
    }

    [Fact]
    public void TryGetError()
    {
        var expectedValue = 3;
        var expectedError = new Error.EntityNotFound(typeof(InfrastructureTestEntity), 4);
        var resultWithValue = expectedValue.ToResult();
        var resultWithError = expectedError.ToResult<int>();

        Assert.False(resultWithValue.TryGetError(out var error, out var value));
        Assert.Null(error);
        Assert.Equal(expectedValue, value);

        Assert.False(resultWithValue.TryGetError(out error));
        Assert.Null(error);


        Assert.True(resultWithError.TryGetError(out error, out value));
        Assert.Equal(expectedError, error);
        Assert.Equal(default, value);

        Assert.True(resultWithError.TryGetError(out error));
        Assert.Equal(expectedError, error);
    }


    private static Result<T> GetResult<T>(T value) where T : notnull => value.ToResult();
    private static Task<Result<T>> GetResultAsync<T>(T value) where T : notnull => Task.FromResult(GetResult(value));
    private static void AssertEqual<T>(T expected, Result<T> actualResult) where T : notnull
    {
        Assert.True(actualResult.TryGetValue(out var actual));
        Assert.Equal(expected, actual);
    }

    
    [Fact]
    public async Task Select()
    {
        AssertEqual(Nothing.Instance, GetResult(3).Select(a => { }));
        AssertEqual(Nothing.Instance, await GetResult(3).Select(async a => await Task.Yield()));
        AssertEqual(4, GetResult(3).Select(a => a + 1));
        AssertEqual(4, GetResult(3).Select(a => GetResult(a + 1)));
        AssertEqual(4, await GetResult(3).Select(async a => await Task.FromResult(a + 1)));
        AssertEqual(4, await GetResult(3).Select(async a => await GetResultAsync(a + 1)));

        AssertEqual(Nothing.Instance, await GetResultAsync(3).Select(a => { }));
        AssertEqual(Nothing.Instance, await GetResultAsync(3).Select(async (int a) => await Task.CompletedTask));
        AssertEqual(4, await GetResultAsync(3).Select(a => a + 1));
        AssertEqual(4, await GetResultAsync(3).Select(a => GetResult(a + 1)));
        AssertEqual(4, await GetResultAsync(3).Select(async a => await Task.FromResult(a + 1)));
        AssertEqual(4, await GetResultAsync(3).Select(async a => await GetResultAsync(a + 1)));
    }

    [Fact]
    public async Task SelectMany()
    {
        AssertEqual(12, await (
            from a in GetResultAsync(3)
            from b in GetResultAsync(4)
            from c in GetResultAsync(5)
            select a + b + c));

        AssertEqual(12, await (
            from a in GetResultAsync(3)
            from b in GetResultAsync(4)
            from c in GetResultAsync(5)
            select GetResult(a + b + c)));

        AssertEqual(12, await (
            from a in GetResultAsync(3)
            from b in GetResultAsync(4)
            from c in GetResultAsync(5)
            select Task.FromResult(a + b + c)));

        AssertEqual(12, await (
            from a in GetResultAsync(3)
            from b in GetResultAsync(4)
            from c in GetResultAsync(5)
            select GetResultAsync(a + b + c)));
    }
}
