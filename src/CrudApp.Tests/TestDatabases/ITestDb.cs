namespace CrudApp.Tests.TestDatabases;
public interface ITestDb
{
    string ConnectionString { get; }
    Task InitializeAsync();
    Task DisposeAsync();
}
