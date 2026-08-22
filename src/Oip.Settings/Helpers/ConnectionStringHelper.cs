using System.Text.RegularExpressions;
using Oip.Settings.Enums;
using Oip.Settings.Models;

namespace Oip.Settings.Helpers;

/// <summary>
/// Helper for connection string
/// </summary>
public static class ConnectionStringHelper
{
    private const RegexOptions ParameterRegexOptions = RegexOptions.IgnoreCase | RegexOptions.Multiline |
                                                       RegexOptions.Singleline | RegexOptions.CultureInvariant;

    /// <summary>
    /// Name of the custom parameter with the provider
    /// </summary>
    public const string XpoProviderParameter = "XpoProvider";

    /// <summary>
    /// Name of the custom parameter enabling EF Core sensitive data logging
    /// </summary>
    public const string SensitiveDataLoggingParameter = "SensitiveDataLogging";

    /// <summary>
    /// Normalizes a connection string by extracting custom parameters
    /// (<see cref="XpoProviderParameter"/>, <see cref="SensitiveDataLoggingParameter"/>)
    /// and returning a ConnectionModel.
    /// </summary>
    /// <param name="connectionString">The connection string to normalize.</param>
    /// <returns>A <see cref="ConnectionModel"/> containing the extracted parameters and the normalized connection string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the connection string is null or empty.</exception>
    public static ConnectionModel NormalizeConnectionString(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentNullException(nameof(connectionString));

        var normalized = connectionString;

        var provider = XpoProvider.InMemoryDataStore;
        if (TryExtractParameter(normalized, XpoProviderParameter, out var providerValue, out normalized))
        {
#if NET6_0_OR_GREATER
            if (Enum.TryParse<XpoProvider>(providerValue, true, out var parsedProvider))
                provider = parsedProvider;
#else
            if (Enum.TryParse(providerValue, true, out XpoProvider parsedProvider))
                provider = parsedProvider;
#endif
        }

        var sensitiveDataLogging = false;
        if (TryExtractParameter(normalized, SensitiveDataLoggingParameter, out var sensitiveValue,
                out normalized))
        {
            sensitiveDataLogging = ParseBoolean(sensitiveValue);
        }

        return new ConnectionModel
        {
            Provider = provider,
            SensitiveDataLogging = sensitiveDataLogging,
            NormalizeConnectionString = normalized,
            ConnectionString = connectionString
        };
    }

    /// <summary>
    /// Extracts a custom parameter from a connection string and returns the connection string without it.
    /// </summary>
    private static bool TryExtractParameter(string connectionString, string parameterName, out string value,
        out string rest)
    {
        var regex = new Regex($@"{Regex.Escape(parameterName)}\s*=(.*?)(;|$)", ParameterRegexOptions);
        var match = regex.Match(connectionString);

        if (!match.Success)
        {
            value = string.Empty;
            rest = connectionString;
            return false;
        }

        value = match.Groups[1].Value.Trim();
        rest = connectionString.Replace(match.Value, string.Empty);
        return true;
    }

    private static bool ParseBoolean(string value)
    {
        if (bool.TryParse(value, out var result))
            return result;

        return value == "1" || string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase);
    }
}
