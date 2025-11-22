using Oip.Settings.Tests.Settings;
namespace Oip.Settings.Tests;

/// <summary>
/// Test fixture for SQLite settings configuration
/// </summary>
[TestFixture]
public class SqliteSettingsTest : BaseSettingsTest
{
    /// <summary>
    /// Tests SQLite settings initialization with development fallback configuration
    /// </summary>
    [Test]
    public void Initialize_WithDevelopmentFallback_ShouldLoadSettings()
    {
        // Arrange
        var appSettingsOptions = new AppSettingsOptions
        {
            JsonFileName = "appsettings-sqlite.json",
            JsonFileNameDevelopment = "appsettings.json"
        };
        // Act
        var instance = SqliteServerAppSettings.Initialize(appSettingsOptions);
        // Assert
        TestBaseSettings(instance);
    }

    /// <summary>
    /// Tests SQLite settings initialization without development fallback configuration
    /// </summary>
    [Test]
    public void Initialize_WithoutDevelopmentFallback_ShouldLoadSettings()
    {
        // Act
        var instance = SqliteServerAppSettings.Initialize(new AppSettingsOptions
        {
            JsonFileName = "appsettings-sqlite.json",
        });
        // Assert
        TestBaseSettings(instance);
    }

    /// <summary>
    /// Represents SQLite server application settings
    /// </summary>
    private class SqliteServerAppSettings : BaseAppSettings<SqliteServerAppSettings>, IBaseSettings
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