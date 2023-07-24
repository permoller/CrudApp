using CrudApp.Infrastructure.OpenApi;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//
// Logging
//
builder.Services.AddLogging(loggingBuilder => loggingBuilder.ClearProviders().AddJsonConsole());


//
// Database
//
builder.Services.AddDbContext<CrudAppDbContext>(dbContextOptionsBuilder =>
{
    dbContextOptionsBuilder.UseSqlite(new SqliteConnection("DataSource=CrudApp.db"));
});


//
// ProblemDetails
//
builder.Services.AddProblemDetails(problemDetailsOptions =>
{
    problemDetailsOptions.CustomizeProblemDetails = problemDetailsContext =>
    {
        problemDetailsContext.ProblemDetails.Extensions.Add("hello", "world");
    };
});


//
// Controllers
//
builder.Services.AddControllers(mvcOptions =>
{

    // Convert exceptions to a problem details response https://tools.ietf.org/html/rfc7807
    mvcOptions.Filters.Add<ProblemDetailsExceptionHandler>();
});


// 
// Lozalization
//
//builder.Services.AddLocalization();


//
// OpenAPI/Swagger UI
//

// Add information about responses for actions.
// Can be overwritten using ProducesResponseType-attribute on individual actions.
builder.Services.AddTransient<IApplicationModelProvider>((_) => new ResponseMetadataProvider("application/json"));


builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(swaggerGenOptions =>
{
    // Make sure non-nullable reference types are not marked as nullable.
    swaggerGenOptions.SupportNonNullableReferenceTypes();

    // Make sure all non-nullable properties are marked as required.
    swaggerGenOptions.SchemaFilter<RequireNonNullablePropertiesSchemaFilter>();
});


//
// Entity ID generation
//
EntityBase.NewEntityId = 
    TimeBasedIdGenerator.NewUsingMillisecondsSince(
            new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
            generatorId: 0// the generator id needs to be different for each instance (consider horizontal scaling and roling updates)
            ).NewId;

var app = builder.Build();


//
// HTTP request pipeline
//
//Microsoft.Extensions.Hosting.EnvironmentName.Development
//Microsoft.Extensions.Hosting.Environments.Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
