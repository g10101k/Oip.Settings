using Oip.Settings.Tests.Settings;

namespace Oip.Settings.Tests;

/// <summary>
/// Test fixture for PostgreSQL settings configuration
/// </summary>
[TestFixture]
public class PostgresSettingsTests : BaseSettingsTest
{
    /// <summary>
    /// Tests that Initialize method correctly sets application settings options
    /// </summary>
    [Test]
    public void Initialize_ShouldSetAppSettingsOptionsCorrectly()
    {
        // Arrange
        var appSettingsOptions = new AppSettingsOptions
        {
            JsonFileName = "appsettings-pg.json",
            JsonFileNameDevelopment = "appsettings.json" // for adding data to database
        };

        // Act
        PostgresSettingsTestsAppSettings.Initialize(appSettingsOptions);
        TestBaseSettings(PostgresSettingsTestsAppSettings.Instance);
    }

    /// <summary>
    /// Tests the Initialize method with AppSettingsOptions
    /// Verifies that the settings are correctly initialized
    /// </summary>
    [Test]
    public void Initialize_WithAppSettingsOptions_ShouldSetSettingsCorrectly()
    {
        // Act
        IBaseSettings instance = PostgresSettingsTestsAppSettings.Initialize(new AppSettingsOptions
        {
            JsonFileName = "appsettings-pg.json",
        });

        // Assert
        TestBaseSettings(instance);
    }

    /// <summary>
    /// Application settings class for PostgreSQL settings tests
    /// </summary>
    private class PostgresSettingsTestsAppSettings : BaseAppSettings<PostgresSettingsTestsAppSettings>, IBaseSettings
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