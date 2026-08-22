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
        Assert.That(instance.ConnectionString.NormalizeConnectionString,
            Is.EqualTo(instance.ConnectionString.ConnectionString));
    }

    [Test]
    public void NormalizeConnectionString_WhenDisabled_KeepsCustomParametersInConnectionString()
    {
        // Arrange
        var options = new AppSettingsOptions
        {
            NormalizeConnectionString = false,
            UseEfCoreProvider = false
        };

        // Act
        var instance = NoNormalizeCustomParametersTestAppSettings.Initialize(options);

        // Assert
        Assert.Multiple(() =>
        {
            // the connection string is passed through untouched, XpoProvider and SensitiveDataLogging are not cut off
            Assert.That(instance.ConnectionString.NormalizeConnectionString,
                Is.EqualTo(NoNormalizeCustomParametersTestAppSettings.RawConnectionString));
            Assert.That(instance.ConnectionString.ConnectionString,
                Is.EqualTo(NoNormalizeCustomParametersTestAppSettings.RawConnectionString));

            // the provider configured on the instance wins over the one written in the connection string
            Assert.That(instance.ConnectionString.Provider, Is.EqualTo(XpoProvider.MSSqlServer));

            // custom parameters are still recognized, they are just not removed from the connection string
            Assert.That(instance.ConnectionString.SensitiveDataLogging, Is.True);
        });
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


    [TestCase("XpoProvider=SQLite;SensitiveDataLogging=true;Data Source=test.db", true)]
    [TestCase("XpoProvider=SQLite;sensitivedatalogging=True;Data Source=test.db", true)]
    [TestCase("XpoProvider=SQLite;SensitiveDataLogging=1;Data Source=test.db", true)]
    [TestCase("XpoProvider=SQLite;SensitiveDataLogging=false;Data Source=test.db", false)]
    [TestCase("XpoProvider=SQLite;Data Source=test.db", false)]
    [TestCase("SensitiveDataLogging=true", true)]
    public void NormalizeConnectionString_ParsesSensitiveDataLogging(string connectionString, bool expected)
    {
        // Act
        var model = ConnectionStringHelper.NormalizeConnectionString(connectionString);

        // Assert
        Assert.That(model.SensitiveDataLogging, Is.EqualTo(expected));
    }

    [Test]
    public void NormalizeConnectionString_RemovesSensitiveDataLoggingFromNormalized()
    {
        // Act
        var model = ConnectionStringHelper.NormalizeConnectionString(
            "XpoProvider=SQLite;SensitiveDataLogging=true;Data Source=test.db");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(model.NormalizeConnectionString, Is.EqualTo("Data Source=test.db"));
            Assert.That(model.ConnectionString,
                Is.EqualTo("XpoProvider=SQLite;SensitiveDataLogging=true;Data Source=test.db"));
        });
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
    /// Settings with a connection string containing custom parameters, used with normalization disabled
    /// </summary>
    private class NoNormalizeCustomParametersTestAppSettings :
        BaseAppSettings<NoNormalizeCustomParametersTestAppSettings>
    {
        public const string RawConnectionString =
            "XpoProvider=Postgres;SensitiveDataLogging=true;Server=localhost;Database=oip-test;";

        public NoNormalizeCustomParametersTestAppSettings()
        {
            ConnectionString = RawConnectionString;
            // provider is configured explicitly, it is not taken from the connection string in this mode
            ConnectionString.Provider = XpoProvider.MSSqlServer;
        }
    }

    /// <summary>
    /// Mock application settings class for testing JSON configuration
    /// </summary>
    private class NormalizeConnectionStringFalseTestAppSettings :
        BaseAppSettings<NormalizeConnectionStringFalseTestAppSettings>, IBaseSettings
    {
        public NormalizeConnectionStringFalseTestAppSettings()
        {
            ConnectionString = Guid.NewGuid().ToString();
            ConnectionString.Provider = XpoProvider.InMemoryDataStore;
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