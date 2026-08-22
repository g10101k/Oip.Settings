using Oip.Settings.Models;

namespace Oip.Settings;

/// <summary>
/// Interface for base application settings class
/// </summary>
public interface IAppSettings
{
    /// <summary>
    /// Connection string in DevExpress format
    /// </summary>
    string ConnectionString { get; set; }

    /// <summary>
    /// Connection string parsed into a model: provider, custom parameters
    /// and connection string without them
    /// </summary>
    ConnectionModel Connection { get; set; }

    /// <summary>
    /// Options to config application settings
    /// </summary>
    AppSettingsOptions AppSettingsOptions { get; }

    /// <summary>
    /// Rebinds the application settings
    /// </summary>
    void Rebind();

    /// <summary>
    /// Saves the current application settings to the database
    /// </summary>
    void SaveSettingsToDb();

    /// <summary>
    /// Returns true when ASPNETCORE_ENVIRONMENT is Development
    /// </summary>
    bool IsDevelopment();
}
