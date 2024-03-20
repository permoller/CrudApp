namespace CrudApp.Infrastructure.Primitives;

public struct Nothing
{
    public static Nothing Select(Action action)
    {
        action();
        return default;
    }
    public static Nothing Select<T>(T t, Action<T> action)
    {
        action(t);
        return default;
    }
    public static async Task<Nothing> Select(Func<Task> action)
    {
        await action();
        return default;
    }
    public static async Task<Nothing> Select<T>(T t, Func<T, Task> action)
    {
        await action(t);
        return default;
    }
}
