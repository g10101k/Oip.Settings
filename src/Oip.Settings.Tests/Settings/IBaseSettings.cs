namespace Oip.Settings.Tests.Settings;

/// <summary>
/// Represents the base interface for settings.
/// </summary>
public interface IBaseSettings
{
    /// <summary>
    /// Gets or sets the connection string.
    /// </summary>
    string ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets a test integer value.
    /// </summary>
    int TestInt { get; set; }

    /// <summary>
    /// Gets or sets a test double value.
    /// </summary>
    double TestDouble { get; set; }

    /// <summary>
    /// Gets or sets a test string value.
    /// </summary>
    string TestString { get; set; }

    /// <summary>
    /// Gets or sets a list of test strings.
    /// </summary>
    List<string> TestStringList { get; set; }

    /// <summary>
    /// Gets or sets a list of base test settings objects.
    /// </summary>
    List<BaseTestSetting> TestObjectList { get; set; }

    /// <summary>
    /// Gets or sets a dictionary of string keys to string values.
    /// </summary>
    Dictionary<string, string> TestDictionary { get; set; }
}