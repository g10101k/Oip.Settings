using Microsoft.Extensions.DependencyInjection;
using Oip.Settings.Attributes;

namespace Oip.Settings.Tests;

[TestFixture]
public class AddObjectPropertiesAsSingletonsTests
{
    [Test]
    public void Throws_When_Instance_Is_Null()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() =>
            services.AddSettingsToDependencyInjection(null));
    }

    [Test]
    public void Registers_Only_Complex_Types()
    {
        var services = new ServiceCollection();
        var settings = new TestSettings();

        services.AddSettingsToDependencyInjection(settings);
        var provider = services.BuildServiceProvider();

        Assert.That(provider.GetService<DummyApplicationSettings>(), Is.Not.Null);
        Assert.That(provider.GetService<DummyApplicationSettingsV3>(), Is.Null);
        Assert.That(provider.GetService<DummyApplicationSettingsV4>(), Is.Null);
    }

    [Test]
    public void Does_Not_Register_Properties_With_Attribute()
    {
        var services = new ServiceCollection();
        var settings = new TestSettings();

        services.AddSettingsToDependencyInjection(settings);
        var provider = services.BuildServiceProvider();

        Assert.That(provider.GetService(settings.IgnoredByAttr.GetType()), Is.Null);
    }

    [Test]
    public void Does_Not_Register_Null_Values()
    {
        var services = new ServiceCollection();
        var settings = new TestSettings { NullComplex = null };

        services.AddSettingsToDependencyInjection(settings);
        var provider = services.BuildServiceProvider();

        var resolved = provider.GetRequiredService<DummyApplicationSettings>();
        Assert.That(resolved, Is.Not.Null); // Only "Complex" should be injected
    }

    [Test]
    public void Does_Not_Register_WriteOnly_Properties()
    {
        var services = new ServiceCollection();
        var settings = new TestSettings();

        Assert.DoesNotThrow(() => { services.AddSettingsToDependencyInjection(settings); });
    }

    [Test]
    public void Registers_Instance_As_Singleton()
    {
        var services = new ServiceCollection();
        var settings = new TestSettings();

        services.AddSettingsToDependencyInjection(settings);
        var provider = services.BuildServiceProvider();

        var instance1 = provider.GetRequiredService<DummyApplicationSettings>();
        var instance2 = provider.GetRequiredService<DummyApplicationSettings>();

        Assert.That(instance1, Is.SameAs(instance2));
    }

    private class DummyApplicationSettings
    {
        public string Name { get; set; } = nameof(DummyApplicationSettings);
    }

    private class DummyApplicationSettingsV2
    {
        public string Name { get; set; } = nameof(DummyApplicationSettingsV2);
    }


    private class DummyApplicationSettingsV3
    {
        public string Name { get; set; } = nameof(DummyApplicationSettingsV3);
    }

    private class DummyApplicationSettingsV4
    {
        public string Name { get; set; } = nameof(DummyApplicationSettingsV4);
    }

    private record DummyRecord(string Name);

    private class TestSettings
    {
        public DummyApplicationSettings Complex { get; set; } = new DummyApplicationSettings() { Name = "visible" };

        public string Name { get; set; } = "str";
        public int IntValue { get; set; } = 42;
        public DateTime Date { get; set; } = DateTime.Now;
        public List<string> List { get; set; } = ["a"];

        [NotAddToDependencyInjection]
        public DummyApplicationSettingsV2 IgnoredByAttr { get; set; } = new() { Name = "not visible" };

        public DummyApplicationSettingsV3 NullComplex { get; set; } = null;

        public DummyApplicationSettingsV4 WriteOnlyProp
        {
            set { }
        }
    }
}