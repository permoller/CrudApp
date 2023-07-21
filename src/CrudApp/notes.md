# Automatic registration of entity types in EF Core
Types that inherit from EntityBase are automatically registered in Entity Framework in CrudAppDbContext.

# Value converters
Properties in the entity types can be marked with custom attributes to add converters that allows storing enums as strings and objects as JSON in the database.
The attributes are detected and the converters added in CrudAppDbContext.

# EF Core Migrations
Requires [dotnet-ef tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) which can be installed using:
<code>dotnet tool install --global dotnet-ef</code>

When the entities have changed a new migration step can be generated using this command from the src-folder:
<code>dotnet ef migrations add *NAME_OF_MIGRATION_STEP* --project .\CrudApp\ --output-dir Database\Migrations</code>


# Optimistic Concurrency Control
The base type for all the entities (EntityBase) has a version-property.
The version-property is automatically updated in CrudAppDbContext when an entity is modified.
The version-property is used by EF Core to do optimistic concurrence control.
This means that when an entity is loaded, modified and saved back to the database, the action will fail if someone else has updated the entity.

??? Does it work if an entity is received in a PUT request, which loads the entity, updates it and then saves the entity ???

# Change tracking


# Simple filter/query functionality


# Authorization

# Exceptions / returning ProblemDetails from the WebAPI
A number of exceptions are defined to indicate client errors.
An exception filter (ProblemDetailsExceptionFilter) handles the exceptions and returns [ProblemDetails](https://datatracker.ietf.org/doc/html/rfc7807) response and the appropiate HTTP status code.

# ASP.NET Integration tests
Automatic integration tests have been made that starts an instance of the service that runs agains an in-memory SQLite database.

[WebApplicationFactory](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-7.0) is used to start the service and modify the service registrations in the IoC container for external dependencies like the database.

# Package dependencies
- Microsoft.EntityFrameworkCore.Sqlite: Sqlite is used as an in-memory database when executing automated tests.
- Microsoft.EntityFrameworkCore.Design: Required when using EF Core Migrations to update the database schema.
- Swashbuckle.AspNetCore: Used for exposing OpenAPI documentation and Swagger UI.

