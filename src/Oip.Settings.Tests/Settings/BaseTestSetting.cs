namespace Oip.Settings.Tests.Settings;

/// <inheritdoc />
public class BaseTestSetting : IBaseSettings
{
    /// <inheritdoc />
    public string ConnectionString { get; set; }

    /// <inheritdoc />
    public int TestInt { get; set; }

    /// <inheritdoc />
    public double TestDouble { get; set; }

    /// <inheritdoc />
    public string TestString { get; set; } = string.Empty;

    /// <inheritdoc />
    public List<string> TestStringList { get; set; }

    /// <inheritdoc />
    public List<BaseTestSetting> TestObjectList { get; set; }

    /// <inheritdoc />
    public Dictionary<string, string> TestDictionary { get; set; }
}