using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using CrudApp.Tests.Infrastructure.WebApi;
using Microsoft.AspNetCore.Hosting;
using CrudApp.Infrastructure.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Diagnostics;

namespace CrudApp.Tests.Infrastructure.Logging;
public class LoggingTests(ITestOutputHelper testOutputHelper, LoggingTests.TestFixture webAppFixture)
    : IntegrationTestsBase<LoggingTests.TestFixture>(testOutputHelper, webAppFixture), IClassFixture<LoggingTests.TestFixture>
{
    public class TestFixture : WebAppFixture
    {
        public List<LogEntry> LogEntries { get; } = new();

        protected override void WithWebHostBuilder(IWebHostBuilder builder)
        {
            base.WithWebHostBuilder(builder);
            builder.ConfigureServices(services =>
            {
                services.TryAddEnumerable(ServiceDescriptor.Singleton<ILogSink>(new LogSinkMock(this)));
            });
        }

        public async override Task StartTestAsync(ITestOutputHelper? testOutputHelper)
        {
            await base.StartTestAsync(testOutputHelper);
            LogEntries.Clear();
        }

        public class LogSinkMock(TestFixture testFixture) : ILogSink
        {
            public void Write(LogEntry logEntry)
            {
                testFixture.LogEntries.Add(logEntry);
            }
        }
    }

    [Fact]
    public async Task TestLogEntryIsCreatedCorrectly()
    {
        var client = Fixture.CreateHttpClient();

        using var activity = new Activity(nameof(TestLogEntryIsCreatedCorrectly)).Start();
        var value = Guid.NewGuid().ToString();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/InfrastructureTest/logging?value={value}");
        request.Headers.Add("traceparent", activity.Id);
        var response = await client.ApiSendAsync(request);
        await response.ApiEnsureSuccessAsync();

        var logEntry = Fixture.LogEntries.Single(l => l.Message == $"Value {value}");

        Assert.Equal("Information", logEntry.Log?.Level);

        Assert.Equal("CrudApp.Infrastructure.Testing.InfrastructureTestController", logEntry.Log?.Logger);

        Assert.Equal(activity.TraceId.ToHexString(), logEntry.Trace?.Id);
        Assert.NotEqual(activity.SpanId.ToHexString(), logEntry.Span?.Id);

        var state = logEntry.State;
        Assert.NotNull(state);
        Assert.Equal(value, state["value"]);

        var scopes = logEntry.Scopes;
        Assert.NotNull(scopes);
        Assert.Equal("Inner method LoggingInnerMethod.", scopes[0].Message);
        Assert.Equal("LoggingInnerMethod", scopes[0].State!["method"]);
        Assert.Equal("inner scope", scopes[1].Message);
        Assert.Equal("outer scope", scopes[2].Message);
        Assert.Equal("Method Logging.", scopes[3].Message);
        Assert.Equal("Logging", scopes[3].State!["method"]);
        Assert.Equal("CrudApp.Infrastructure.Testing.InfrastructureTestController.Logging (CrudApp)", scopes[4].State!["ActionName"]);
        Assert.Equal("/api/InfrastructureTest/logging", scopes[5].State!["RequestPath"]);

        var labels = logEntry.Labels;
        Assert.NotNull(labels);
        Assert.Equal(value, labels["value"]);
        Assert.Equal("LoggingInnerMethod", labels["method"]);
        Assert.Equal("CrudApp.Infrastructure.Testing.InfrastructureTestController.Logging (CrudApp)", labels["ActionName"]);
        Assert.Equal("/api/InfrastructureTest/logging", labels["RequestPath"]);
    }
}
