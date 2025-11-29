using System.Text.RegularExpressions;
using Oip.Settings.Enums;
using Oip.Settings.Models;

namespace Oip.Settings.Helpers;

/// <summary>
/// Helper for connection string
/// </summary>
public static class ConnectionStringHelper
{
    /// <summary>
    /// Normalizes a connection string by extracting the XpoProvider and returning a ConnectionModel.
    /// </summary>
    /// <param name="connectionString">The connection string to normalize.</param>
    /// <returns>A <see cref="ConnectionModel"/> containing the extracted provider and the normalized connection string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the connection string is null or empty.</exception>
    public static ConnectionModel NormalizeConnectionString(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentNullException(nameof(connectionString));
        var provider = XpoProvider.InMemoryDataStore;
        var regex = new Regex(@"XpoProvider\s*=(.*?);", (RegexOptions)531);
        var matches = regex.Matches(connectionString);

        if (matches.Count == 0)
        {
            return new ConnectionModel { Provider = provider, NormalizeConnectionString = connectionString };
        }

        connectionString = connectionString.Replace(matches[0].Value, string.Empty);
        provider = (XpoProvider)Enum.Parse(typeof(XpoProvider), matches[0].Groups[1].Value);

        return new ConnectionModel { Provider = provider, NormalizeConnectionString = connectionString };
    }
}