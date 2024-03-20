using CrudApp.Infrastructure.Logging;
using CrudApp.Infrastructure.WebApi;
using Microsoft.AspNetCore.HttpLogging;

public class Program
{
    static Program()
    {
        EntityBase.NewEntityId =
            TimeBasedIdGenerator.NewUsingMillisecondsSince(
                    new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    generatorId: 0// the generator id needs to be different for each instance (consider horizontal scaling)
                    ).NewId;
    }
    

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder
            .Services
            .AddCrudAppLogging(builder.Configuration)
            .AddCrudAppExceptionHandling()
            .AddCrudAppAuthentication()
            .AddCrudAppApiConvetions()
            //.AddLocalization()
            .AddCrudAppDbContext(builder.Configuration)
            .AddCrudAppJsonOptions()
            // Add services for logging incomming requests with Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware
            //.AddHttpLogging(_ => {})
            // Add services for logging incomming requests to files in src/CrudApp/logs
            //.AddW3CLogging(_ => {})
            // Add services for logging incomming http requests
            .AddCrudAppIncommingHttpRequestLogging()
            // Add services for logging outgoing http requests made with HttpClient
            .AddCrudAppOutgoingHttpRequestLogging()
            .AddControllers();

        // Add logging of outgoing requests to the default HttpClient
        builder.Services.AddHttpClient(string.Empty).UseCrudAppOutgoingHttpRequestLogging();


        var app = builder.Build();

        if (app.Environment.IsDevelopment())
            ProblemDetailsHelper.IncludeExceptionInProblemDetails = true;

        // Log incoming requsts with Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware
        // app.UseHttpLogging();

        // Log incomming requests to files
        // app.UseW3CLogging();
        
        // Log incomming requests
        app.UseCrudAppIncommingHttpRequestLogging();

        app.UseHttpsRedirection();
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseRouting();
        app.UseAuthentication();
        app.UseCrudAppAuthenticationContext();
        app.UseCrudAppAuthorizationContext();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }

}
