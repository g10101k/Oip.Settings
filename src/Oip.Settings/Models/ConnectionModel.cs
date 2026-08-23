using System.ComponentModel;
using Oip.Settings.Converters;
using Oip.Settings.Enums;
using Oip.Settings.Helpers;

namespace Oip.Settings.Models;

/// <summary>
/// Connection model for connection string as DevExpress.
/// Can be bound directly from a plain connection string in configuration,
/// for example <c>"ConnectionString": "XpoProvider=SQLite;SensitiveDataLogging=true;Data Source=settings.db"</c>.
/// </summary>
[TypeConverter(typeof(ConnectionModelTypeConverter))]
public class ConnectionModel
{
    /// <summary>
    /// Provider
    /// </summary>
    public XpoProvider Provider { get; set; } = XpoProvider.InMemoryDataStore;

    /// <summary>
    /// Enables EF Core sensitive data logging.
    /// Set in configuration with the custom parameter <c>SensitiveDataLogging=true;</c>
    /// </summary>
    public bool SensitiveDataLogging { get; set; }

    /// <summary>
    /// Connection string without custom parameters (XpoProvider, SensitiveDataLogging)
    /// </summary>
    public string NormalizeConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Original connection string as it was written in configuration
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Parses a connection string in DevExpress format into a <see cref="ConnectionModel"/>
    /// </summary>
    /// <param name="connectionString">Connection string to parse</param>
    /// <returns>Parsed connection model</returns>
    public static ConnectionModel Parse(string connectionString)
    {
        return ConnectionStringHelper.NormalizeConnectionString(connectionString);
    }

    /// <summary>
    /// Converts a connection string to a <see cref="ConnectionModel"/>
    /// </summary>
    public static implicit operator ConnectionModel(string connectionString) => Parse(connectionString);

    /// <summary>
    /// Converts a <see cref="ConnectionModel"/> to the original connection string,
    /// exactly as it was written in configuration, custom parameters included.
    /// To open a connection use <see cref="NormalizeConnectionString"/> instead.
    /// </summary>
    public static implicit operator string(ConnectionModel? model) => model?.ToString() ?? string.Empty;

    /// <summary>
    /// Returns the original connection string
    /// </summary>
    public override string ToString() => ConnectionString ?? NormalizeConnectionString ?? string.Empty;
}