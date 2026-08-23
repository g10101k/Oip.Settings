using Microsoft.Extensions.Configuration;
using Oip.Settings.Enums;
using Oip.Settings.Models;

namespace Oip.Settings.Tests;

/// <summary>
/// Tests for binding a plain connection string into a <see cref="ConnectionModel"/>
/// </summary>
[TestFixture]
public class ConnectionModelTests
{
    /// <summary>
    /// Test that a string value in configuration is bound to a ConnectionModel property
    /// </summary>
    [Test]
    public void Bind_ShouldConvertStringToConnectionModel()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionString"] = "XpoProvider=Postgres;Server=localhost;Database=oip-test;",
                ["ReportConnection"] = "XpoProvider=MSSqlServer;Server=localhost;Database=report;"
            })
            .Build();

        var instance = new ConnectionModelTestSettings();
        configuration.Bind(instance);

        Assert.Multiple(() =>
        {
            Assert.That(instance.ReportConnection, Is.Not.Null);
            Assert.That(instance.ReportConnection!.Provider, Is.EqualTo(XpoProvider.MSSqlServer));
            Assert.That(instance.ReportConnection.NormalizeConnectionString,
                Is.EqualTo("Server=localhost;Database=report;"));
            Assert.That(instance.ReportConnection.ConnectionString,
                Is.EqualTo("XpoProvider=MSSqlServer;Server=localhost;Database=report;"));
        });
    }

    /// <summary>
    /// Test that AppSettings exposes the parsed connection model
    /// </summary>
    [Test]
    public void Instance_ShouldExposeConnectionModel()
    {
        var settings = ConnectionModelAppSettings.Initialize(new AppSettingsOptions
        {
            JsonFileName = "appsettings-sqlite.json",
            UseEfCoreProvider = false
        });

        Assert.Multiple(() =>
        {
            Assert.That(settings.ConnectionString.Provider, Is.EqualTo(XpoProvider.SQLite));
            Assert.That(settings.ConnectionString.NormalizeConnectionString, Is.EqualTo("Data Source=settings.db"));
            Assert.That(settings.ConnectionString.ConnectionString,
                Is.EqualTo("XpoProvider=SQLite;Data Source=settings.db"));
        });
    }

    /// <summary>
    /// Test that a model is implicitly converted to the original connection string
    /// </summary>
    [Test]
    public void ImplicitOperator_ShouldReturnOriginalConnectionString()
    {
        const string connectionString = "XpoProvider=SQLite;SensitiveDataLogging=true;Data Source=settings.db";
        ConnectionModel model = connectionString;

        string implicitValue = model;

        Assert.Multiple(() =>
        {
            Assert.That(implicitValue, Is.EqualTo(connectionString));
            Assert.That(implicitValue, Is.EqualTo(model.ToString()));
            Assert.That(model.NormalizeConnectionString, Is.EqualTo("Data Source=settings.db"));
        });
    }

    /// <summary>
    /// Test that a null model is converted to an empty string instead of throwing
    /// </summary>
    [Test]
    public void ImplicitOperator_ShouldReturnEmptyStringForNull()
    {
        ConnectionModel nullModel = null!;

        string implicitValue = nullModel;

        Assert.That(implicitValue, Is.Empty);
    }

    private class ConnectionModelTestSettings
    {
        public ConnectionModel? ReportConnection { get; set; }
    }

    private class ConnectionModelAppSettings : BaseAppSettings<ConnectionModelAppSettings>;
}
