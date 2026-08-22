using Microsoft.Extensions.DependencyInjection;
using Oip.Settings.Attributes;

namespace Oip.Settings.Tests.Extensions;

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

        Assert.That(provider.GetService<ComplexSettings>(), Is.Not.Null);
        Assert.That(provider.GetService<ListWrapperSettings>(), Is.Not.Null);
        Assert.That(provider.GetService<DictionaryWrapperSettings>(), Is.Not.Null);

        Assert.That(provider.GetService<NullComplexSettings>(), Is.Null);
        Assert.That(provider.GetService<WriteOnlyPropSettings>(), Is.Null);
        Assert.That(provider.GetService<IgnoredByAttrSettings>(), Is.Null);
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

        var resolved = provider.GetRequiredService<ComplexSettings>();
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

        var instance1 = provider.GetRequiredService<ComplexSettings>();
        var instance2 = provider.GetRequiredService<ComplexSettings>();

        Assert.That(instance1, Is.SameAs(instance2));
    }

    private class ComplexSettings
    {
        public string Name { get; set; } = nameof(ComplexSettings);
    }

    private class IgnoredByAttrSettings
    {
        public string Name { get; set; } = nameof(IgnoredByAttrSettings);
    }


    private class NullComplexSettings
    {
        public string Name { get; set; } = nameof(NullComplexSettings);
    }

    private class WriteOnlyPropSettings
    {
        public string Name { get; set; } = nameof(WriteOnlyPropSettings);
    }

    private record DummyRecord(string Name);

    private class TestSettings
    {
        public ComplexSettings Complex { get; set; } = new ComplexSettings() { Name = "visible" };

        public string Name { get; set; } = "str";
        public int IntValue { get; set; } = 42;
        public DateTime Date { get; set; } = DateTime.Now;
        public List<string> List { get; set; } = ["a"];

        [NotAddToDependencyInjection]
        public IgnoredByAttrSettings IgnoredByAttr { get; set; } = new() { Name = "not visible" };

        public NullComplexSettings NullComplex { get; set; } = null;

        public WriteOnlyPropSettings WriteOnlyProp
        {
            set { }
        }

        public DummyRecord RecordProp { get; set; } = new("RecordProp");
        public ListWrapperSettings ListWrapper { get; set; } = new();
        public DictionaryWrapperSettings DictionaryWrapper { get; set; } = new();
    }

    private class ListWrapperSettings : List<string>;
    private class DictionaryWrapperSettings : Dictionary<string, string>;
}