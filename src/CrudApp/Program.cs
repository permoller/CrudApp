using CrudApp.Infrastructure.Database;
using CrudApp.Infrastructure.Entity;
using CrudApp.Infrastructure.ErrorHandling;
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
    // On exception we return a problem details response https://tools.ietf.org/html/rfc7807.
    mvcOptions.Filters.Add<ProblemDetailsExceptionFilter>();
});


// 
// Lozalization
//
//builder.Services.AddLocalization();


//
// OpenAPI/Swagger UI
//
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
