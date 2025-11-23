namespace Oip.Settings.Attributes;

/// <summary>
/// Indicates that a property should not be registered as a singleton service.
/// This attribute is used during dependency injection to exclude specific properties
/// from being added to the service collection.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class NotAddToDependencyInjectionAttribute : Attribute;