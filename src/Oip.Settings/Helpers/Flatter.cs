using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Oip.Settings.Attributes;

namespace Oip.Settings.Helpers;

/// <summary>
/// Provides static methods for converting application settings instances to dictionaries
/// </summary>
public static class Flatter
{
    /// <summary>
    /// Converts an application settings instance to a flat dictionary
    /// </summary>
    /// <param name="obj">Object instance to flatten</param>
    public static Dictionary<string, string> ToDictionary(object obj)
    {
        var visitedObjects = new HashSet<object>();
        var dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        ToDictionaryInternal(dictionary, obj, string.Empty, visitedObjects);
        return dictionary;
    }

    /// <summary>
    /// Internal recursive method with cycle detection for flattening objects
    /// </summary>
    /// <param name="dictionary">Target dictionary to populate</param>
    /// <param name="obj">Current object being processed</param>
    /// <param name="prefix">Current key prefix</param>
    /// <param name="visitedObjects">Set of visited objects for cycle detection</param>
    private static void ToDictionaryInternal(Dictionary<string, string> dictionary, object? obj, string prefix,
        HashSet<object> visitedObjects)
    {
        if (obj == null) return;

        // Check for circular reference
        if (!visitedObjects.Add(obj))
        {
            throw new InvalidOperationException(
                $"Circular reference detected while serializing object of type {obj.GetType().Name}");
        }

        try
        {
            var type = obj.GetType();
            
            // Handle dictionaries
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                DictionaryToDictionary(dictionary, obj, prefix, visitedObjects);
                return;
            } 
            
            // Handle lists and other collections
            if (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
            {
                ListToDictionary(dictionary, obj, prefix, visitedObjects);
                return;
            }

            var fields = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (field.GetCustomAttribute(typeof(NotSaveToDbAttribute)) != null)
                    continue;

                var value = field.GetValue(obj);
                if (value == null) continue;

                var key = string.IsNullOrEmpty(prefix) ? field.Name : $"{prefix}:{field.Name}";

                if (IsSimpleOrNull(value))
                {
                    dictionary.Add(key, ToStringInvariant(value));
                }
                else
                {
                    ToDictionaryInternal(dictionary, value, key, visitedObjects);
                }
            }
        }
        finally
        {
            // Remove object from visited set when exiting recursion
            visitedObjects.Remove(obj);
        }
    }

    /// <summary>
    /// Handles conversion of dictionaries to dictionary entries
    /// </summary>
    private static void DictionaryToDictionary(Dictionary<string, string> dictionary, object dict, string prefix,
        HashSet<object> visitedObjects)
    {
        if (dict is IDictionary dictionary1)
        {
            foreach (DictionaryEntry keyValue in dictionary1)
            {
                var key = string.IsNullOrEmpty(prefix) 
                    ? $"{keyValue.Key}" 
                    : $"{prefix}:{keyValue.Key}";

                if (IsSimpleOrNull(keyValue.Value))
                {
                    dictionary.Add(key, ToStringInvariant(keyValue.Value));
                }
                else
                {
                    ToDictionaryInternal(dictionary, keyValue.Value!, key, visitedObjects);
                }
            }
        }
    }

    /// <summary>
    /// Handles conversion of lists and collections to dictionary entries
    /// </summary>
    private static void ListToDictionary(Dictionary<string, string> dictionary, object list, string prefix,
        HashSet<object> visitedObjects)
    {
        var i = 0;
        foreach (var item in (IEnumerable)list)
        {
            var key = $"{prefix}:{i}";
            
            if (IsSimpleOrNull(item))
            {
                dictionary.Add(key, ToStringInvariant(item));
            }
            else
            {
                ToDictionaryInternal(dictionary, item, key, visitedObjects);
            }

            i++;
        }
    }

    /// <summary>
    /// Converts object to string using invariant culture for formattable types
    /// </summary>
    /// <param name="obj">Object to convert to string</param>
    /// <returns>String representation of the object</returns>
    private static string ToStringInvariant(object? obj)
    {
        return obj switch
        {
            null => string.Empty,
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => obj.ToString() ?? string.Empty
        };
    }

    /// <summary>
    /// Determines if the object is a simple type or null
    /// </summary>
    /// <param name="obj">Object to check</param>
    /// <returns>True if object is simple type or null, otherwise false</returns>
    private static bool IsSimpleOrNull(object? obj)
    {
        if (obj is null) return true;
        var type = obj.GetType();
        return type.IsPrimitive ||
               type.IsEnum ||
               type == typeof(string) ||
               type == typeof(decimal) ||
               type == typeof(DateTime) ||
               type == typeof(DateTimeOffset) ||
               type == typeof(TimeSpan) ||
               type == typeof(Guid) ||
               HasStringTypeConverter(type);
    }

    /// <summary>
    /// Determines if the type can be stored as a plain string, like <see cref="Oip.Settings.Models.ConnectionModel"/>
    /// </summary>
    /// <param name="type">Type to check</param>
    /// <returns>True when the type has a two way string type converter</returns>
    private static bool HasStringTypeConverter(Type type)
    {
        var converter = TypeDescriptor.GetConverter(type);
        return converter.CanConvertFrom(typeof(string)) && converter.CanConvertTo(typeof(string));
    }
}