using Oip.Settings.Enums;
using Oip.Settings.Tests.Common;
using Oip.Settings.Tests.Settings;

namespace Oip.Settings.Tests;

/// <summary>
/// Test fixture for JSON application settings functionality
/// </summary>
[TestFixture]
public class JsonAppSettingsTests : BaseSettingsTest
{
    /// <summary>
    /// Set up test environment before each test
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        JsonTestAppSettings.Initialize();
    }

    /// <summary>
    /// Test base settings functionality through the instance
    /// </summary>
    [Test]
    public void Instance_TestIBaseSettings()
    {
        TestBaseSettings(JsonTestAppSettings.Instance);
    }

    /// <summary>
    /// Test the singleton behavior of the Instance property
    /// </summary>
    [Test]
    public void Instance_ShouldReturnSameInstance_WhenAccessedMultipleTimes()
    {
        // Arrange
        var firstInstance = JsonTestAppSettings.Instance;
        var secondInstance = JsonTestAppSettings.Instance;

        // Assert
        Assert.That(firstInstance, Is.SameAs(secondInstance));
    }

    /// <summary>
    /// Test configuration binding
    /// </summary>
    [Test]
    public void BindConfig_ShouldBindCorrectly()
    {
        var settings = JsonTestAppSettings.Instance;
        Assert.That(settings.ConnectionString, Is.Null);
        Assert.That(settings.NormalizedConnectionString, Is.Null);
    }

    /// <summary>
    /// Test connection string normalization
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
    /// Test the Initialize method with parameters
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
    /// Test that the singleton instance is created even when Initialize is not called
    /// </summary>
    [Test]
    public void Instance_ShouldCreateInstanceEvenIfNotInitializedExplicitly()
    {
        // Act
        var instance = JsonTestAppSettings.Instance;

        // Assert
        Assert.That(instance, Is.Not.Null);
    }

    [Test]
    public void Instance_TestTwoInstances()
    {
        var instance = JsonTestAppSettings.Initialize(new AppSettingsOptions());

        Assert.That(instance, Is.EqualTo(JsonTestAppSettings.Instance));

        var instance2 = JsonTestAppSettings.Initialize(new AppSettingsOptions()
        {
            JsonFileName = "custom.json",
        });

        Assert.That(instance, Is.Not.EqualTo(instance2));
    }


    /// <summary>
    /// Mock application settings class for testing JSON configuration
    /// </summary>
    private class JsonTestAppSettings : BaseAppSettings<JsonTestAppSettings>, IBaseSettings
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