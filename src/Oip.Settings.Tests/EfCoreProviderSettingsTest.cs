using Oip.Settings.Helpers;
using Oip.Settings.Tests.Settings;

namespace Oip.Settings.Tests;

/// <summary>
/// Test fixture for SQLite settings configuration
/// </summary>
[TestFixture(true, "appsettings-sqlite.json")]
[TestFixture(false, "appsettings-sqlite.json")]
[TestFixture(true, "appsettings-sql-server.json")]
[TestFixture(false, "appsettings-sql-server.json")]
[TestFixture(true, "appsettings-pg.json")]
[TestFixture(false, "appsettings-pg.json")]
public class EfCoreProviderSettingsTest : BaseSettingsTest
{
    private readonly bool _useJsonStorage;
    private readonly string _testSettingsFile;
    private const string DevelopmentSettingsFile = "appsettings.json";
    private const int ModifiedTestIntValue = 34;

    public EfCoreProviderSettingsTest(bool useJsonStorage, string appSettingsJson)
    {
        _useJsonStorage = useJsonStorage;
        _testSettingsFile = appSettingsJson;
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
            JsonFileName = _testSettingsFile,
            JsonFileNameDevelopment = DevelopmentSettingsFile,
            UseJsonStorage = _useJsonStorage
        };

        // Act
        var instance = AppSettings.Initialize(appSettingsOptions);

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
        var instance = AppSettings.Initialize(new AppSettingsOptions
        {
            JsonFileName = _testSettingsFile,
            UseJsonStorage = _useJsonStorage
        });

        // Assert
        TestBaseSettings(instance);
        Assert.That(instance, Is.Not.Null, "Settings instance should not be null");
    }

    [Test, Order(3)]
    public void Initialize_ChangeDbSettingsAndReload_ShouldReflectChanges()
    {
        using var context = AppSettings.GetAppSettingsContext();
        if (_useJsonStorage)
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