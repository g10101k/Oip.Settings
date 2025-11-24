using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Oip.Settings.Attributes;

namespace Oip.Settings.Providers;

/// <summary>
/// JSON converter for <see cref="IAppSettings"/> classes, excluding properties marked with
/// <see cref="Oip.Settings.Attributes.NotSaveToDbAttribute"/> or <see cref="System.Text.Json.Serialization.JsonIgnoreAttribute"/>.
/// </summary>
/// <typeparam name="T">The type of settings class to convert.</typeparam>
public class AppSettingsJsonConverter<T> : JsonConverter<T> where T : class
{
    /// <inheritdoc />
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var tempOptions = new JsonSerializerOptions(options);
        tempOptions.Converters.Remove(this);

        return JsonSerializer.Deserialize<T>(ref reader, tempOptions);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (property.GetCustomAttribute<JsonIgnoreAttribute>() != null)
                continue;

            if (property.GetCustomAttribute<NotSaveToDbAttribute>() != null)
                continue;
            var propertyName = options.PropertyNamingPolicy?.ConvertName(property.Name) ?? property.Name;
            var propertyValue = property.GetValue(value);
            writer.WritePropertyName(propertyName);
            JsonSerializer.Serialize(writer, propertyValue, options);
        }

        writer.WriteEndObject();
    }
}