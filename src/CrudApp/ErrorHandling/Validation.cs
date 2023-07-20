using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace CrudApp.ErrorHandling;

public static class Validation
{

    public static void ThrowIfNotValid(this ModelStateDictionary? modelState)
    {
        if (modelState?.IsValid == false)
            throw new ValidationException(modelState);
    }

    public static void Required<T>([NotNull] T value, [CallerArgumentExpression(nameof(value))] string? name = default)
    {
        Required(null, value, name).ThrowIfNotValid();
    }

    public static ModelStateDictionary? Required<T>(this ModelStateDictionary? modelState, T value, [CallerArgumentExpression(nameof(value))] string? name = default)
    {
        if (Equals(value, default(T)) || (value is string s && string.IsNullOrWhiteSpace(s)))
        {
            if (modelState is null)
                modelState = new ModelStateDictionary();
            modelState.AddModelError(name!,"Required.");
        }
        return modelState;
    }
    public static void Equal<T>(T valueA, T valueB, IEqualityComparer<T>? equalityComparer = null, [CallerArgumentExpression(nameof(valueA))] string? nameA = default, [CallerArgumentExpression(nameof(valueB))] string? nameB = default)
    {
        Equal(null, valueA, valueB, equalityComparer, nameA, nameB).ThrowIfNotValid();
    }
    
    public static ModelStateDictionary? Equal<T>(this ModelStateDictionary? modelState, T valueA, T valueB, IEqualityComparer<T>? equalityComparer = null, [CallerArgumentExpression(nameof(valueA))] string? nameA = default, [CallerArgumentExpression(nameof(valueB))] string? nameB = default)
    {
        if (equalityComparer is null)
            equalityComparer = EqualityComparer<T>.Default;
        if (!equalityComparer.Equals(valueA, valueB))
        {
            if (modelState is null)
                modelState = new ModelStateDictionary();
            modelState.AddModelError(nameA!, $"Must be equal to {nameB}.");
            modelState.AddModelError(nameB!, $"Must be equal to {nameA}.");
        }
        return modelState;
    }

}
