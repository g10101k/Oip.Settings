using Oip.Settings.Enums;
using Oip.Settings.Tests.Settings;

namespace Oip.Settings.Tests;

[TestFixture]
public class JsonAppSettingsTests
{
    [SetUp]
    public void SetUp()
    {
        JsonTestAppSettings.Initialize(useEfCoreProvider: false, normalizeConnectionString: true);
    }

    /// <summary>
    /// Test the singleton behavior of the Instance property.
    /// Ensures that multiple calls return the same instance.
    /// </summary>
    [Test]
    public void Instance_ShouldReturnSameInstance_WhenAccessedMultipleTimes()
    {
        // Arrange
        var firstInstance = JsonTestAppSettings.Instance;
        var secondInstance = JsonTestAppSettings.Instance;

        // Assert
        Assert.That(firstInstance, Is.SameAs(secondInstance)); // Assert that both references point to the same instance
    }

    /// <summary>
    /// Test configuration binding.
    /// Verifies that configuration values are correctly bound to the instance.
    /// </summary>
    [Test]
    public void BindConfig_ShouldBindCorrectly()
    {
        var settings = JsonTestAppSettings.Instance;

        Assert.That("XpoProvider=Postgres;Server=localhost;Database=oip-test;uid=postgres;pwd=postgres;",
            Is.EqualTo(settings.ConnectionString));
        Assert.That(settings.NormalizedConnectionString,
            Is.EqualTo("Server=localhost;Database=oip-test;uid=postgres;pwd=postgres;"));
    }

    /// <summary>
    /// Test connection string normalization.
    /// Verifies that the normalization logic works correctly when enabled.
    /// </summary>
    [Test]
    public void NormalizeConnectionString_ShouldNormalizeCorrectly_WhenFlagIsTrue()
    {
        // Act
        var settings = JsonTestAppSettings.Instance;

        // Assert
        Assert.That(XpoProvider.Postgres, Is.EqualTo(settings.Provider));
        Assert.That(settings.NormalizedConnectionString,
            Is.EqualTo("Server=localhost;Database=oip-test;uid=postgres;pwd=postgres;"));
    }

    /// <summary>
    /// Test the Initialize method with parameters.
    /// Verifies that all properties are correctly set when provided via parameters.
    /// </summary>
    [Test]
    public void Initialize_WithParameters_ShouldSetOptionsCorrectly()
    {
        // Arrange
        var programArgs = new string[] { "--debug=true", "--env=prod" };

        // Act
        var instance = JsonTestAppSettings.Initialize(programArguments: programArgs,
            useEfCoreProvider: false, jsonFileName: "custom.json",
            jsonFileNameDevelopment: "custom.Development.json");

        // Assert
        Assert.That(programArgs, Is.EqualTo(instance.AppSettingsOptions.ProgramArguments));
        Assert.That("custom.json", Is.EqualTo(instance.AppSettingsOptions.JsonFileName));
        Assert.That("custom.Development.json", Is.EqualTo(instance.AppSettingsOptions.JsonFileNameDevelopment));
        Assert.That(instance.AppSettingsOptions.UseEfCoreProvider, Is.False);
    }

    /// <summary>
    /// Test that the singleton instance is created even when Initialize is not called.
    /// Ensures that the Instance property will automatically initialize the settings.
    /// </summary>
    [Test]
    public void Instance_ShouldCreateInstanceEvenIfNotInitializedExplicitly()
    {
        // Act
        var instance = JsonTestAppSettings.Instance;

        // Assert
        Assert.That(instance, Is.Not.Null); // Ensure the instance is created
    }

    /// <summary>
    /// A mock class that simulates application settings for testing.
    /// </summary>
    private class JsonTestAppSettings : BaseAppSettings<JsonTestAppSettings>, IBaseSettings
    {
        public int TestInt { get; set; }
        public double TestDouble { get; set; }
        public string TestString { get; set; } = null!;
        public List<string> TestStringList { get; set; }
        public List<BaseSettingObject> TestObjectList { get; set; }
        public Dictionary<string, string> TestDictionary { get; set; }
    }

    private class WithNormalizedAppSettings : BaseAppSettings<WithNormalizedAppSettings>, IBaseSettings
    {
        public int TestInt { get; set; }
        public double TestDouble { get; set; }
        public string TestString { get; set; }
        public List<string> TestStringList { get; set; }
        public List<BaseSettingObject> TestObjectList { get; set; }
        public Dictionary<string, string> TestDictionary { get; set; }
    }

    private class SqliteAppSettings : BaseAppSettings<SqliteAppSettings>, IBaseSettings
    {
        public int Test { get; set; } = 1;
        public int TestInt { get; set; }
        public double TestDouble { get; set; }
        public string TestString { get; set; }
        public List<string> TestStringList { get; set; }
        public List<BaseSettingObject> TestObjectList { get; set; }
        public Dictionary<string, string> TestDictionary { get; set; }
    }
}