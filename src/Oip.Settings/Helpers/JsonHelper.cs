using System.Text.Json;
using Oip.Settings.Providers;

namespace Oip.Settings.Helpers;

/// <summary>
/// Provides static methods for serializing and deserializing application settings to and from JSON format.
/// </summary>
/// <typeparam name="TAppSettings">The type of application settings class.</typeparam>
public static class JsonHelper<TAppSettings> where TAppSettings : class, IAppSettings
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new AppSettingsJsonConverter<TAppSettings>() }
    };

    /// <summary>
    /// Deserializes a JSON string to an instance of <typeparamref name="TAppSettings"/>.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>An instance of <typeparamref name="TAppSettings"/>, or null if deserialization fails.</returns>
    public static TAppSettings? FromJson(string json)
    {
        return JsonSerializer.Deserialize<TAppSettings>(json, JsonOptions);
    }

    /// <summary>
    /// Serializes an instance of <typeparamref name="TAppSettings"/> to a JSON string.
    /// </summary>
    /// <param name="appSettings">The instance of <typeparamref name="TAppSettings"/> to serialize.</param>
    /// <returns>A JSON string representing the object.</returns>
    public static string ToJson(TAppSettings appSettings)
    {
        return JsonSerializer.Serialize(appSettings, JsonOptions);
    }
}