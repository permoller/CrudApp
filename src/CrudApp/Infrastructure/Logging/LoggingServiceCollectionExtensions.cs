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

        // Only register services if a url to OpenSearch is provided
        if (string.IsNullOrEmpty(configuration[$"{nameof(OpenSearchOptions)}:{nameof(OpenSearchOptions.BaseAddress)}"]))
            return services;

        // Configure options
        services.AddOptions<OpenSearchOptions>()
            .Bind(configuration.GetSection(nameof(OpenSearchOptions)))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Register the buffer where log entries are saved until they are send to OpenSearch
        services.AddSingleton<OpenSearchBufferLogSink>();

        // Make sure we get the existing OpenSearchBufferLogSink singleton instance when injecting ILogSink
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ILogSink, OpenSearchBufferLogSink>(sp => sp.GetRequiredService<OpenSearchBufferLogSink>()));

        // Add background service to send log entries to OpenSearch
        services.AddHostedService<OpenSearchSender>();

        // Configure HTTP client used when sending log entries to OpenSearch
        services.AddHttpClient(OpenSearchSender.HttpClientName, (sp, client) => {
            var options = sp.GetRequiredService<IOptions<OpenSearchOptions>>();
            client.BaseAddress = options.Value.BaseAddress;
        });
        

        return services;
    }

    
}
