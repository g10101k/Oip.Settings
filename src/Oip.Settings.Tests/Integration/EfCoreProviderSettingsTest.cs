using Oip.Settings.Helpers;
using Oip.Settings.Tests.Common;
using Oip.Settings.Tests.Settings;

namespace Oip.Settings.Tests.Integration;

/// <summary>
/// Test fixture for SQLite settings configuration
/// </summary>
[TestFixture(true, "appsettings-sqlite.json")]
[TestFixture(false, "appsettings-sqlite.json")]
[TestFixture(true, "appsettings-sql-server.json", Category = TestCategories.Integration)]
[TestFixture(false, "appsettings-sql-server.json", Category = TestCategories.Integration)]
[TestFixture(true, "appsettings-pg.json", Category = TestCategories.Integration)]
[TestFixture(false, "appsettings-pg.json", Category = TestCategories.Integration)]
public class EfCoreProviderSettingsTest(bool useJsonStorage, string appSettingsJson) : BaseSettingsTest
{
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
            JsonFileName = appSettingsJson,
            JsonFileNameDevelopment = DevelopmentSettingsFile,
            UseJsonStorage = useJsonStorage
        };

        // Act
        var instance = AppSettings.Initialize(appSettingsOptions);

        // Assert
        TestBaseSettings(instance);
    }

    /// <summary>
    /// Tests SQLite settings initialization without development fallback configuration
    /// </summary>
    [Test, Order(2)]
    public void Initialize_WithoutDevelopmentFallback_ShouldLoadSettings()
    {
        // Arrange & Act
        var instance = AppSettings.Initialize(new AppSettingsOptions
        {
            JsonFileName = appSettingsJson,
            UseJsonStorage = useJsonStorage
        });

        TestBaseSettings(instance);
    }

    [Test, Order(3)]
    public void Initialize_ChangeDbSettingsAndReload_ShouldReflectChanges()
    {
        using var context = AppSettings.GetAppSettingsContext();
        if (useJsonStorage)
        {
            var originalSetting = context.AppSettings.First(x => x.Key == typeof(AppSettings).FullName);

            var settings = JsonHelper<AppSettings>.FromJson(originalSetting.Value) ??
                           throw new InvalidOperationException(
                               $"Cant convert from json to appsettings: {originalSetting.Value}");
            settings.TestInt = ModifiedTestIntValue;
            originalSetting.Value = JsonHelper<AppSettings>.ToJson(settings);
            context.SaveChanges();
        }
        else
        {
            var originalSetting = context.AppSettings.First(x => x.Key == "TestInt");
            originalSetting.Value = ModifiedTestIntValue.ToString();
        }

        context.SaveChanges();

        // Act - Reload settings
        AppSettings.Instance.Rebind();

        // Assert
        Assert.That(AppSettings.Instance.TestInt, Is.EqualTo(ModifiedTestIntValue),
            "Settings should reflect database changes after reload");
    }

    [Test, Order(4)]
    public void Initialize_ChangeAndSave_ShouldReflectChanges()
    {
        const int newTestIntValue = 112358;
        AppSettings.Instance.TestInt = newTestIntValue;

        // Act - Reload settings
        AppSettings.Instance.SaveSettingsToDb();

        // Assert
        Assert.That(AppSettings.Instance.TestInt, Is.EqualTo(newTestIntValue),
            "Settings should reflect database changes after save and inner rebind");
        // Arrange & Act
        var instance = AppSettings.Initialize(new AppSettingsOptions
        {
            JsonFileName = appSettingsJson,
            UseJsonStorage = useJsonStorage
        });
        Assert.That(instance.TestInt, Is.EqualTo(newTestIntValue), "Settings should reflect database changes after reload");
        Assert.That(AppSettings.Instance.TestInt, Is.EqualTo(newTestIntValue),
            "Settings should reflect database changes after reload");
    }
    
    

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        using var context = AppSettings.GetAppSettingsContext();
        if (!context.AppSettings.Any()) return;
        context.AppSettings.RemoveRange(context.AppSettings);
        context.SaveChanges();
    }

    /// <summary>
    /// Represents SQLite server application settings
    /// </summary>
    private class AppSettings : BaseAppSettings<AppSettings>, IBaseSettings
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