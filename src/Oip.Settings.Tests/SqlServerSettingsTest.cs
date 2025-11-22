using Oip.Settings.Tests.Settings;

namespace Oip.Settings.Tests;

/// <summary>
/// Test fixture for SQL Server settings configuration
/// </summary>
[TestFixture]
public class SqlServerSettingsTest : BaseSettingsTest
{
    /// <summary>
    /// Test the Initialize method with AppSettingsOptions.
    /// Verifies that the settings are correctly initialized.
    /// </summary>
    [Test]
    public void Initialize_ShouldSetAppSettingsOptionsCorrectly()
    {
        // Arrange
        var appSettingsOptions = new AppSettingsOptions
        {
            JsonFileName = "appsettings-sql-server.json",
            JsonFileNameDevelopment = "appsettings.json",
        };
        // Act
        var instance = SqlServerAppSettings.Initialize(appSettingsOptions);
        // Assert
        Assert.That(instance, Is.Not.Null);
        TestBaseSettings(instance);
    }

    /// <summary>
    /// Test the Initialize method without development configuration
    /// Verifies that the settings are correctly initialized when development config is not provided
    /// </summary>
    [Test]
    public void Initialize_WithoutDevelopmentConfig_ShouldLoadSettingsCorrectly()
    {
        // Arrange
        var appSettingsOptions = new AppSettingsOptions
        {
            JsonFileName = "appsettings-sql-server.json",
        };
        // Act
        var instance = SqlServerAppSettings.Initialize(appSettingsOptions);
        // Assert
        Assert.That(instance, Is.Not.Null);
        TestBaseSettings(instance);
    }

    /// <summary>
    /// SQL Server application settings implementation for testing
    /// </summary>
    private class SqlServerAppSettings : BaseAppSettings<SqlServerAppSettings>, IBaseSettings
    {
        /// <inheritdoc />
        public int TestInt { get; set; }

        /// <inheritdoc />
        public double TestDouble { get; set; }

        /// <inheritdoc />
        public string TestString { get; set; } = null!;

        /// <inheritdoc />
        public List<string> TestStringList { get; set; }

        /// <inheritdoc />
        public List<BaseTestSetting> TestObjectList { get; set; }

        /// <inheritdoc />
        public Dictionary<string, string> TestDictionary { get; set; }
    }
}