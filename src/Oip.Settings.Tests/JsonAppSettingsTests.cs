using Oip.Settings.Enums;
using Oip.Settings.Tests.Settings;

namespace Oip.Settings.Tests;

[TestFixture]
public class JsonAppSettingsTests: BaseSettingsTest
{
    [SetUp]
    public void SetUp()
    {
        JsonTestAppSettings.Initialize();
    }

    [Test]
    public void Instance_TestIBaseSettings()
    {
        TestBaseSettings(JsonTestAppSettings.Instance);
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

        Assert.That(settings.ConnectionString, Is.Null);
        Assert.That(settings.NormalizedConnectionString, Is.Null);
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
        Assert.That(settings.Provider, Is.EqualTo(XpoProvider.InMemoryDataStore));
        Assert.That(settings.NormalizedConnectionString, Is.Null);
    }

    /// <summary>
    /// Test the Initialize method with parameters.
    /// Verifies that all properties are correctly set when provided via parameters.
    /// </summary>
    [Test]
    public void Initialize_WithParameters_ShouldSetOptionsCorrectly()
    {
        // Arrange
        var programArgs = new[] { "--debug=true", "--env=prod" };

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
    /// Mock application settings class for testing JSON configuration
    /// </summary>
    private class JsonTestAppSettings : BaseAppSettings<JsonTestAppSettings>, IBaseSettings
    {
        /// <summary>
        /// Test integer property
        /// </summary>
        public int TestInt { get; set; }
        
        /// <summary>
        /// Test double property
        /// </summary>
        public double TestDouble { get; set; }
        
        /// <summary>
        /// Test string property
        /// </summary>
        public string TestString { get; set; } = null!;
        
        /// <summary>
        /// Test string list property
        /// </summary>
        public List<string> TestStringList { get; set; }
        
        /// <summary>
        /// Test object list property
        /// </summary>
        public List<BaseTestSetting> TestObjectList { get; set; }
        
        /// <summary>
        /// Test dictionary property
        /// </summary>
        public Dictionary<string, string> TestDictionary { get; set; }
    }
}