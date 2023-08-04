namespace CrudApp.Infrastructure.UtilityCode;

public static class ExceptionUtils
{
    public static IEnumerable<Exception> GetExceptionsRecursively(this Exception? exception)
    {
        if (exception == null)
            yield break;

        yield return exception;
        if (exception is AggregateException aggregateException && aggregateException.InnerExceptions is not null)
        {
            foreach(var ex in aggregateException.InnerExceptions.SelectMany(e => e.GetExceptionsRecursively()))
                yield return ex;
        }
        else
        {
            foreach(var ex in exception.InnerException.GetExceptionsRecursively())
                yield return ex;
        }
    }

    public static IEnumerable<string> GetExceptionMessagesRecursively(this Exception? exception)
    {
        return exception.GetExceptionsRecursively().Select(e => e.Message);
    }
}
