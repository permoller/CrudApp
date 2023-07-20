namespace CrudApp.ChangeTracking;

/// <summary>
/// Use this attribute to skip changetracking on entire entity classes or individual properties.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public class SkipChangeTrackingAttribute : Attribute
{
}
