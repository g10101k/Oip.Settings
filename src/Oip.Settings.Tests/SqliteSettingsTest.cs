using Oip.Settings.Tests.Settings;

namespace Oip.Settings.Tests;

[TestFixture]
public class SqliteSettingsTest : BaseSettingsTest
{
    [Test]
    public void Sqlite_Test()
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

    [Test]
    public void Sqlite_Test2()
    {
        // Act
        var instance = SqliteServerAppSettings.Initialize(new AppSettingsOptions
        {
            JsonFileName = "appsettings-sqlite.json",
        });

        // Assert
        TestBaseSettings(instance);
    }

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