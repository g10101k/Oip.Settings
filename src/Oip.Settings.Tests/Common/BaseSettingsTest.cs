using Oip.Settings.Tests.Settings;

namespace Oip.Settings.Tests.Common;

public class BaseSettingsTest
{
    protected static void TestBaseSettings(IBaseSettings settings)
    {
        Assert.That(settings, Is.Not.Null);
        Assert.That(settings.TestInt, Is.EqualTo(100000)); // appsettings-sqlite.json 
        Assert.That(settings.TestDouble, Is.EqualTo(3.14d));
        Assert.That(settings.TestString, Is.EqualTo("TestString"));
        Assert.That(settings.TestObjectList, Is.Not.Null);
        Assert.That(settings.TestObjectList.Count, Is.EqualTo(1));
        Assert.That(settings.TestObjectList[0], Is.Not.Null);

        Assert.That(settings.TestStringList, Is.Not.Null);
        Assert.That(settings.TestStringList.Count, Is.EqualTo(3));
        Assert.That(settings.TestStringList, Is.EquivalentTo(["test1", "test2", "test3"]));

        var firstObject = settings.TestObjectList[0];
        Assert.That(firstObject.ConnectionString,
            Is.EqualTo("Server=localhost;Database=oip-test;uid=postgres;pwd=postgres;"));
        Assert.That(firstObject.TestInt, Is.EqualTo(100000));
        Assert.That(firstObject.TestDouble, Is.EqualTo(3.14d));
        Assert.That(firstObject.TestString, Is.EqualTo("TestString"));

        Assert.That(firstObject.TestStringList, Is.Not.Null);
        Assert.That(firstObject.TestStringList.Count, Is.EqualTo(3));
        Assert.That(firstObject.TestStringList,
            Is.EquivalentTo(["TestStringList1", "TestStringList2", "TestStringList3"]));

        Assert.That(settings.TestDictionary, Is.Not.Null);
        Assert.That(settings.TestDictionary.Count, Is.EqualTo(3));
        Assert.That(settings.TestDictionary["test"], Is.EqualTo("test"));
        Assert.That(settings.TestDictionary["test1"], Is.EqualTo("test1"));
        Assert.That(settings.TestDictionary["test2"], Is.EqualTo("test2"));
    }
}