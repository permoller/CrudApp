using Microsoft.AspNetCore.Mvc;

namespace CrudApp.Infrastructure.Testing;

/// <summary>
/// This is used to test the application infrastructure.
/// </summary>
public class InfrastructureTestController : EntityControllerBase<InfrastructureTestEntity>
{
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
    public InfrastructureTestEntity NotNullInt()
    {
        // Used for testing the status code returned from an action when an object is returned.
        return new(new());
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
}
