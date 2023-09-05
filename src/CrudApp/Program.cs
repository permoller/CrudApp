using CrudApp.Infrastructure.Logging;
using CrudApp.Infrastructure.OpenApi;
using CrudApp.Infrastructure.UtilityCode;

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
            .AddCrudAppOpenApi()
            //.AddLocalization()
            .AddCrudAppDbContext()
            .AddCrudAppJsonOptions()
            .AddHttpLogging(_ => {})
            .AddControllers();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
            ApiExceptionHandler.IsExceptionDetailsInResponseEnabled = true;
        

        // Change the enabled log level of Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware to log incomming requests and responses.
        app.UseHttpLogging();

        // Simple logging of the incomming requests. It only logs a single line once the request is done.
        app.UseMiddleware<IncommingHttpRequestLoggingMiddleware>();

        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseCrudAppAuthenticationContext();
        app.UseCrudAppAuthorizationContext();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }

}
