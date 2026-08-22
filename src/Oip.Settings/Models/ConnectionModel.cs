using System.ComponentModel;
using Oip.Settings.Converters;
using Oip.Settings.Enums;
using Oip.Settings.Helpers;

namespace Oip.Settings.Models;

/// <summary>
/// Connection model for connection string as DevExpress.
/// Can be bound directly from a plain connection string in configuration,
/// for example <c>"ConnectionString": "XpoProvider=SQLite;Data Source=settings.db"</c>.
/// </summary>
[TypeConverter(typeof(ConnectionModelTypeConverter))]
public class ConnectionModel
{
    /// <summary>
    /// Provider
    /// </summary>
    public XpoProvider Provider { get; set; }

    /// <summary>
    /// Connection string without XpoProvider
    /// </summary>
    public string NormalizeConnectionString { get; set; } = default!;

    /// <summary>
    /// Original connection string as it was written in configuration
    /// </summary>
    public string ConnectionString { get; set; } = default!;

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
    /// Returns the original connection string
    /// </summary>
    public override string ToString() => ConnectionString ?? NormalizeConnectionString ?? string.Empty;
}
