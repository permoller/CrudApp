using Microsoft.AspNetCore.Mvc;

namespace CrudApp.Infrastructure.Testing;

/// <summary>
/// This is used to test the application infrastructure.
/// </summary>
public class InfrastructureTestController : EntityControllerBase<InfrastructureTestEntity>
{
    private readonly ILogger<InfrastructureTestController> _logger;

    public InfrastructureTestController(ILogger<InfrastructureTestController> logger)
    {
        _logger = logger;
    }

    [Route("void"), HttpGet, HttpPut, HttpPost, HttpDelete]
    public void Void()
    {
        // Used for testing the status code returned from a void action.
    }

    [Route("null-int"), HttpGet, HttpPut, HttpPost, HttpDelete]
    public int? NullInt()
    {
        // Used for testing the status code returned from an action when null is returned.
        return null;
    }

    [Route("not-null-int"), HttpGet, HttpPut, HttpPost, HttpDelete]
    public int NotNullInt()
    {
        // Used for testing the status code returned from an action when an object is returned.
        return 1;
    }

    [Route("null-ref"), HttpGet, HttpPut, HttpPost, HttpDelete]
    public InfrastructureTestEntity? NullRef()
    {
        // Used for testing the status code returned from an action when null is returned.
        return null;
    }
    
    [Route("not-null-ref"), HttpGet, HttpPut, HttpPost, HttpDelete]
    public InfrastructureTestEntity NotNullRef()
    {
        // Used for testing the status code returned from an action when an object is returned.
        return new(new());
    }

    [Route("logging"), HttpGet]
    public string Logging(string value)
    {
        using (var _ = _logger.BeginScope("Method {method}.", nameof(Logging)))
        using (var __ = _logger.BeginScope("outer scope"))
        using (var ___ = _logger.BeginScope("inner scope"))
        {
            return LoggingInnerMethod(value);
        }
    }

    private string LoggingInnerMethod(string value)
    {
        using (var _ = _logger.BeginScope("Inner method {method}.", nameof(LoggingInnerMethod)))
            _logger.LogInformation("Value {value}", value);
        return value;
    }
}
