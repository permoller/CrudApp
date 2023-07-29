using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Runtime.Serialization;

namespace CrudApp.Infrastructure.ErrorHandling;

/// <summary>
/// Indicates the request did not succeed due to an invalid model (bad request).
/// Contains errors per property.
/// </summary>
[Serializable]
public sealed class ValidationException : Exception
{
    public ValidationException(ModelStateDictionary modelState) : base()
    {
        ModelState = modelState;
    }

    private ValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        ModelState = (ModelStateDictionary)info.GetValue(nameof(ModelState), typeof(ModelStateDictionary))!;
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue(nameof(ModelState), ModelState);
        base.GetObjectData(info, context);
    }
    public ModelStateDictionary ModelState { get; }
}

