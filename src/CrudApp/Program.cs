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
            .AddLogging(loggingBuilder => {
                loggingBuilder.ClearProviders();
#if DEBUG
                // Do not use structured logging in local development environment. It is hard to read.
                loggingBuilder.AddConsole();
#else
        loggingBuilder.AddJsonConsole();
#endif
            })
            .AddCrudAppExceptionHandling()
            .AddCrudAppAuthentication()
            .AddCrudAppOpenApi()
            //.AddLocalization()
            .AddCrudAppDbContext()
            .AddCrudAppJsonOptions()
            .AddControllers();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            ApiExceptionHandler.IsExceptionDetailsInResponseEnabled = true;
        }
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
