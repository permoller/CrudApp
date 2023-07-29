using CrudApp.Infrastructure.Authentication;
using CrudApp.Infrastructure.OpenApi;

var builder = WebApplication.CreateBuilder(args);


builder
    .Services
    .AddLogging(loggingBuilder => loggingBuilder.ClearProviders().AddJsonConsole())
    .AddCrudAppExceptionHandling()
    .AddCrudAppAuthentication()
    .AddCrudAppOpenApi()
    //.AddLocalization()
    .AddCrudAppDbContext()
    .AddControllers();


EntityBase.NewEntityId = 
    TimeBasedIdGenerator.NewUsingMillisecondsSince(
            new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
            generatorId: 0// the generator id needs to be different for each instance (consider horizontal scaling)
            ).NewId;

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseCrudAppAuthorizationContext();
app.UseAuthorization();
app.MapControllers();
app.Run();
