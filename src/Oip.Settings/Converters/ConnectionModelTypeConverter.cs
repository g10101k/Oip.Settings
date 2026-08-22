using System.ComponentModel;
using System.Globalization;
using Oip.Settings.Models;

namespace Oip.Settings.Converters;

/// <summary>
/// Allows <see cref="ConnectionModel"/> properties to be bound from a plain connection string
/// written in appsettings.json, environment variables, command line, secrets or database.
/// </summary>
public class ConnectionModelTypeConverter : TypeConverter
{
    /// <inheritdoc />
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    /// <inheritdoc />
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
    }

    /// <inheritdoc />
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string connectionString)
        {
            return string.IsNullOrEmpty(connectionString) ? null : ConnectionModel.Parse(connectionString);
        }

        return base.ConvertFrom(context, culture, value);
    }

    /// <inheritdoc />
    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value,
        Type destinationType)
    {
        if (destinationType == typeof(string))
            return value?.ToString() ?? string.Empty;

        return base.ConvertTo(context, culture, value, destinationType);
    }
}
