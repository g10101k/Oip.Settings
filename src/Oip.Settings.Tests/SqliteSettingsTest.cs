using Microsoft.EntityFrameworkCore;
using Oip.Settings.Contexts;
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
    [Test, Order(1)]
    public void Initialize_WithDevelopmentFallback_ShouldLoadSettings()
    {
        // Arrange
        var appSettingsOptions = new AppSettingsOptions
        {
            JsonFileName = "appsettings-sqlite.json",
            JsonFileNameDevelopment = "appsettings.json"
        };
        // Act
        var instance = SqliteAppSettings.Initialize(appSettingsOptions);
        // Assert
        TestBaseSettings(instance);
    }

    /// <summary>
    /// Tests SQLite settings initialization without development fallback configuration
    /// </summary>
    [Test, Order(2)]
    public void Initialize_WithoutDevelopmentFallback_ShouldLoadSettings()
    {
        // Act
        var instance = SqliteAppSettings.Initialize(new AppSettingsOptions
        {
            JsonFileName = "appsettings-sqlite.json",
        });
        // Assert
        TestBaseSettings(instance);
    }

    [Test, Order(3)]
    public void Initialize_ИзменитьНастройкиБД_ИПеречитатьЗаново()
    {
        using var context = SqliteAppSettings.GetAppSettingsContext() ?? throw new NullReferenceException();

        context.AppSettings.First(x => x.Key == "TestInt").Value = "34";
        context.SaveChanges();

        SqliteAppSettings.Instance.Rebind();

        Assert.That(SqliteAppSettings.Instance.TestInt, Is.EqualTo(34));
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        using var context = SqliteAppSettings.GetAppSettingsContext() ?? throw new NullReferenceException();

        context.AppSettings.RemoveRange(context.AppSettings);
    }

    /// <summary>
    /// Represents SQLite server application settings
    /// </summary>
    private class SqliteAppSettings : BaseAppSettings<SqliteAppSettings>, IBaseSettings
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