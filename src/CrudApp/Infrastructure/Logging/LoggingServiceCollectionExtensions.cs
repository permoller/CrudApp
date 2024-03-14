using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace CrudApp.Infrastructure.Logging;

public static class LoggingServiceCollectionExtensions
{
    public static IServiceCollection AddCrudAppLogging(this IServiceCollection services, ConfigurationManager configuration)
    {
        services
            .AddLogging(loggingBuilder => loggingBuilder.ClearProviders())
            .AddSinkLoggerProvider()
            .AddTextWriterSink(Console.Out, TextWriterLogSink.Format.PlainText)
            .AddOpenSearchSink(configuration);

        return services;
    }

    private static IServiceCollection AddSinkLoggerProvider(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, SinkLoggerProvider>());
        return services;
    }

    private static IServiceCollection AddTextWriterSink(this IServiceCollection services, TextWriter textWriter, TextWriterLogSink.Format format)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ILogSink>(new TextWriterLogSink(textWriter, format)));
        return services;
    }

    private static IServiceCollection AddOpenSearchSink(this IServiceCollection services, ConfigurationManager configuration)
    {
        // The OpenSearch sink is made from a buffer where log entries are collected and a background service that sends the collected log entries to the OpenSearch bulk endpoint.

        services.AddSingleton<OpenSearchBufferLogSink>();
        // Make sure we get the same OpenSearchBuffer singleton instance when injecting ILogSink
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ILogSink, OpenSearchBufferLogSink>(sp => sp.GetRequiredService<OpenSearchBufferLogSink>()));
        
        services.AddHostedService<OpenSearchSender>();
        services.AddOptions<OpenSearchOptions>()
            .Bind(configuration.GetSection(nameof(OpenSearchOptions)))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddHttpClient(OpenSearchSender.HttpClientName, (sp, client) => {
            var options = sp.GetRequiredService<IOptions<OpenSearchOptions>>();
            client.BaseAddress = new Uri(options.Value.BaseAddress);
        });

        return services;
    }

    
}
