namespace CrudApp.Infrastructure.Primitives;

public static class TaskExtensions
{
    public static async Task<A> Select<A>(this Task task, Func<A> selectA) { await task; return selectA(); }

    public static async Task<B> Select<A, B>(this Task<A> taskA, Func<A, B> selectB) => selectB(await taskA);

    public static async Task<B> Select<A, B>(this Task<A> taskA, Func<A, Task<B>> selectTaskB) => await selectTaskB(await taskA);

}
