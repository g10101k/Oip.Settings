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
    private const string TestSettingsFile = "appsettings-sqlite.json";
    private const string DevelopmentSettingsFile = "appsettings.json";
    private const int ModifiedTestIntValue = 34;

    /// <summary>
    /// Tests SQLite settings initialization with development fallback configuration
    /// </summary>
    [Test, Order(1)]
    public void Initialize_WithDevelopmentFallback_ShouldLoadSettings()
    {
        // Arrange
        var appSettingsOptions = new AppSettingsOptions
        {
            JsonFileName = TestSettingsFile,
            JsonFileNameDevelopment = DevelopmentSettingsFile
        };

        // Act
        var instance = SqliteAppSettings.Initialize(appSettingsOptions);

        // Assert
        TestBaseSettings(instance);
        Assert.That(instance, Is.Not.Null, "Settings instance should not be null");
    }

    /// <summary>
    /// Tests SQLite settings initialization without development fallback configuration
    /// </summary>
    [Test, Order(2)]
    public void Initialize_WithoutDevelopmentFallback_ShouldLoadSettings()
    {
        // Arrange & Act
        var instance = SqliteAppSettings.Initialize(new AppSettingsOptions
        {
            JsonFileName = TestSettingsFile,
        });

        // Assert
        TestBaseSettings(instance);
        Assert.That(instance, Is.Not.Null, "Settings instance should not be null");
    }

    [Test, Order(3)]
    public void Initialize_ChangeDbSettingsAndReload_ShouldReflectChanges()
    {
        // Arrange
        using var context = SqliteAppSettings.GetAppSettingsContext();
        var originalSetting = context.AppSettings.First(x => x.Key == "TestInt");

        // Act - Modify setting
        originalSetting.Value = ModifiedTestIntValue.ToString();
        context.SaveChanges();

        // Act - Reload settings
        SqliteAppSettings.Instance.Rebind();

        // Assert
        Assert.That(SqliteAppSettings.Instance.TestInt, Is.EqualTo(ModifiedTestIntValue),
            "Settings should reflect database changes after reload");

        // Cleanup - Restore original value
        originalSetting.Value = "0"; // or whatever the original value was
        context.SaveChanges();
        SqliteAppSettings.Instance.Rebind();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        CleanupTestData();
    }

    private static void CleanupTestData()
    {
        using var context = SqliteAppSettings.GetAppSettingsContext();

        if (context.AppSettings.Any())
        {
            context.AppSettings.RemoveRange(context.AppSettings);
            context.SaveChanges();
        }
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