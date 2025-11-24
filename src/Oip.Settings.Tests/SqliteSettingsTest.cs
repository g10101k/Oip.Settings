using Microsoft.EntityFrameworkCore;
using Oip.Settings.Contexts;
using Oip.Settings.Tests.Settings;

namespace Oip.Settings.Tests;

/// <summary>
/// Test fixture for SQLite settings configuration
/// </summary>
[TestFixture(true)]
[TestFixture(false)]
public class SqliteSettingsTest : BaseSettingsTest
{
    private readonly bool _useJsonStorage;
    private const string TestSettingsFile = "appsettings-sqlite.json";
    private const string DevelopmentSettingsFile = "appsettings.json";
    private const int ModifiedTestIntValue = 34;

    public SqliteSettingsTest(bool useJsonStorage)
    {
        _useJsonStorage = useJsonStorage;
    }

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
            JsonFileNameDevelopment = DevelopmentSettingsFile,
            UseJsonStorage = _useJsonStorage
        };

        // Act
        var instance = SqliteAppSettings.Initialize(appSettingsOptions);

        // Assert
        Assert.That(instance, Is.Not.Null, "Settings instance should not be null");

        TestBaseSettings(instance);
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
            UseJsonStorage = _useJsonStorage
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