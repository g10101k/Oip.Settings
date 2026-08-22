using Oip.Settings.Enums;
using Oip.Settings.Helpers;
using Oip.Settings.Tests.Settings;

namespace Oip.Settings.Tests.Helpers;

[TestFixture]
public class NormalizeConnectionStringTests
{
    [Test]
    public void NormalizeConnectionString_WhenDisabled_ReturnsOriginalConnectionString()
    {
        // Arrange
        var options = new AppSettingsOptions()
        {
            NormalizeConnectionString = false,
        };

        // Act
        var instance = NormalizeConnectionStringFalseTestAppSettings.Initialize(options);

        // Assert
        Assert.That(instance, Is.Not.Null);
        Assert.That(instance.NormalizedConnectionString, Is.EqualTo(instance.ConnectionString));
    }

    [TestCase("XpoProvider=InMemoryDataStore;", XpoProvider.InMemoryDataStore)]
    [TestCase("XpoProvider=MSSqlServer;", XpoProvider.MSSqlServer)]
    [TestCase("XpoProvider=Postgres;", XpoProvider.Postgres)]
    [TestCase("XpoProvider=SQLite;", XpoProvider.SQLite)]
    [TestCase("XpoProvider=SQLite;Data Source=test.db", XpoProvider.SQLite)]
    [TestCase("  XpoProvider=MSSqlServer;  ", XpoProvider.MSSqlServer)]
    public void NormalizeConnectionString_WithVariousInputs_ReturnsExpectedProvider(string connectionString,
        XpoProvider expectedProvider)
    {
        // Act
        var model = ConnectionStringHelper.NormalizeConnectionString(connectionString);

        // Assert
        Assert.That(model.Provider, Is.EqualTo(expectedProvider));
    }


    [TestCase("   ")]
    [TestCase("InvalidProvider=Test;")]
    public void NormalizeConnectionString_WithInvalidInput_ReturnsExpectedDefault(string connectionString)
    {
        // Act & Assert
        // Assuming the method should handle invalid input gracefully
        Assert.DoesNotThrow(() => ConnectionStringHelper.NormalizeConnectionString(connectionString));
    }

    [TestCase(null)]
    [TestCase("")]
    public void NormalizeConnectionString_WithNullOrEmptyInput_ThrowsArgumentNullException(string connectionString)
    {
        Assert.Catch<ArgumentNullException>(() => ConnectionStringHelper.NormalizeConnectionString(connectionString));
    }
    
    [TestCase("XpoProvider=MSSqlServer;Server=localhost", "Server=localhost")]
    public void NormalizeConnectionString_PreservesOtherParameters(string connectionString, string expectedParams)
    {
        // Act
        var model = ConnectionStringHelper.NormalizeConnectionString(connectionString);
    
        // Assert
        Assert.That(model.NormalizeConnectionString, Does.Contain(expectedParams));
    }

    [TestCase("xpoprovider=MSSqlServer;", XpoProvider.MSSqlServer)]
    public void NormalizeConnectionString_IsCaseInsensitive(string connectionString, XpoProvider expectedProvider)
    {
        // Act
        var model = ConnectionStringHelper.NormalizeConnectionString(connectionString);
    
        // Assert
        Assert.That(model.Provider, Is.EqualTo(expectedProvider));
    }
    
    [Test]
    public void NormalizeConnectionString_WithInvalidProvider_ReturnsUnknownProvider()
    {
        // Arrange
        var connectionString = "InvalidProvider=Test;";
    
        // Act
        var model = ConnectionStringHelper.NormalizeConnectionString(connectionString);
    
        // Assert
        Assert.That(model.Provider, Is.EqualTo(XpoProvider.InMemoryDataStore));
    }

    /// <summary>
    /// Mock application settings class for testing JSON configuration
    /// </summary>
    private class NormalizeConnectionStringFalseTestAppSettings :
        BaseAppSettings<NormalizeConnectionStringFalseTestAppSettings>, IBaseSettings
    {
        public NormalizeConnectionStringFalseTestAppSettings()
        {
            Provider = XpoProvider.InMemoryDataStore;
            ConnectionString = Guid.NewGuid().ToString();
        }

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