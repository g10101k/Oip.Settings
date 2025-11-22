using Oip.Settings.Tests.Settings;

namespace Oip.Settings.Tests;

[TestFixture]
public class PostgresSettingsTests : BaseSettingsTest
{
    [SetUp]
    public void SetUp()
    {
        // Arrange
        var appSettingsOptions = new AppSettingsOptions
        {
            JsonFileName = "appsettings-pg.json",
            JsonFileNameDevelopment = "appsettings.json" // for add data to database
        };

        // Act
        PostgresSettingsTestsAppSettings.Initialize(appSettingsOptions);
    }

    [Test]
    public void Initialize_ShouldSetAppSettingsOptionsCorrectly_()
    {
        TestBaseSettings(PostgresSettingsTestsAppSettings.Instance);
    }


    /// <summary>
    /// Test the Initialize method with AppSettingsOptions.
    /// Verifies that the settings are correctly initialized.
    /// </summary>
    [Test]
    public void Initialize_ShouldSetAppSettingsOptionsCorrectly()
    {
        IBaseSettings instance = PostgresSettingsTestsAppSettings.Initialize(new AppSettingsOptions
        {
            JsonFileName = "appsettings-pg.json",
        });

        // Assert
        TestBaseSettings(instance);
    }

    private class PostgresSettingsTestsAppSettings : BaseAppSettings<PostgresSettingsTestsAppSettings>, IBaseSettings
    {
        public int TestInt { get; set; }
        public double TestDouble { get; set; }
        public string TestString { get; set; }
        public List<string> TestStringList { get; set; }
        public List<BaseTestSetting> TestObjectList { get; set; }
        public Dictionary<string, string> TestDictionary { get; set; }
    }
}