using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc;

namespace CrudApp.Infrastructure.ErrorHandling;

/// <summary>
/// Indicates the request did not succeed due to an invalid model (bad request).
/// Contains errors per property in a <see cref="ModelStateDictionary"/>.
/// Transformed to a <see cref="ValidationProblemDetails"/> response in <see cref="ApiExceptionHandler"/>.
/// </summary>
public sealed class ValidationException : Exception
{
    public ValidationException(ModelStateDictionary modelState) : base()
    {
        ModelState = modelState;
    }

    public ModelStateDictionary ModelState
    {
        get => (ModelStateDictionary)Data[nameof(ModelState)]!;
        set => Data[nameof(ModelState)] = value;
    }
}

