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

    public static string GetMessagesIncludingData(this Exception exception, Predicate<Exception>? includeMessage = null)
    {
        var sb = new StringBuilder();
        bool prefixWithNewLine = false;
        foreach(var e in exception.GetExceptionsRecursively())
        {
            if (includeMessage is null || includeMessage(e))
            {
                if (prefixWithNewLine)
                    sb.AppendLine();
                prefixWithNewLine = true;

                e.AppendMessageWithData(sb);
            }
        }
        return sb.ToString();
    }

    public static StringBuilder AppendMessageWithData(this Exception exception, StringBuilder sb)
    {
        sb.Append(exception.Message);
        if (exception.Data.Count > 0)
        {
            sb.Append(" [");
            var seperator = "";
            foreach (var key in exception.Data.Keys)
            {
                sb.Append(seperator).Append(key).Append("=").Append(exception.Data[key]);
                seperator = ", ";
            }
            sb.Append("]");
        }
        return sb;
    }
}
