using Oip.Settings.Tests.Settings;

namespace Oip.Settings.Tests;

[TestFixture]
public class SqlServerSettingsTest
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
        };

        // Act
        var instance = SqliteAppSettings.Initialize(appSettingsOptions);

        // Assert
        Assert.That(instance, Is.Not.Null);
    }

    private class SqliteAppSettings : BaseAppSettings<SqliteAppSettings>, IBaseSettings
    {
        public int TestInt { get; set; }
        public double TestDouble { get; set; }
        public string TestString { get; set; }
        public List<string> TestStringList { get; set; }
        public List<BaseTestSetting> TestObjectList { get; set; }
        public Dictionary<string, string> TestDictionary { get; set; }
    }
}