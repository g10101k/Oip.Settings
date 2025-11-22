using Oip.Settings.Enums;
using Oip.Settings.Tests.Settings;

namespace Oip.Settings.Tests;

[TestFixture]
public class SqliteSettingsTest
{
    [Test]
    public void Sqlite_Test()
    {
        // Arrange
        var appSettingsOptions = new AppSettingsOptions
        {
            JsonFileName = "appsettings-sqlite.json",
        };

        // Act
        var instance = SqliteServerAppSettings.Initialize(appSettingsOptions);

        // Assert
        Assert.That(instance, Is.Not.Null);
        Assert.That(instance.TestInt, Is.EqualTo(100000)); // appsettings-sqlite.json 
        Assert.That(instance.TestDouble, Is.EqualTo(3.14d));
        Assert.That(instance.TestString, Is.EqualTo("TestString"));
        Assert.That(instance.TestObjectList, Is.Not.Null);
        Assert.That(instance.TestObjectList.Count, Is.EqualTo(1));
        Assert.That(instance.TestObjectList[0], Is.Not.Null);

        Assert.That(instance.TestStringList, Is.Not.Null);
        Assert.That(instance.TestStringList.Count, Is.EqualTo(3));
        Assert.That(instance.TestStringList, Is.EquivalentTo(["test1", "test2", "test3"]));

        var firstObject = instance.TestObjectList[0];
        Assert.That(firstObject.ConnectionString,
            Is.EqualTo("XpoProvider=Postgres;Server=localhost;Database=oip-test;uid=postgres;pwd=postgres;"));
        Assert.That(firstObject.TestInt, Is.EqualTo(100000));
        Assert.That(firstObject.TestDouble, Is.EqualTo(3.14d));
        Assert.That(firstObject.TestString, Is.EqualTo("TestString"));

        Assert.That(firstObject.TestStringList, Is.Not.Null);
        Assert.That(firstObject.TestStringList.Count, Is.EqualTo(3));
        Assert.That(firstObject.TestStringList,
            Is.EquivalentTo(["TestStringList1", "TestStringList2", "TestStringList3"]));

        Assert.That(instance.TestDictionary, Is.Not.Null);
        Assert.That(instance.TestDictionary.Count, Is.EqualTo(3));
        Assert.That(instance.TestDictionary["test"], Is.EqualTo("test"));
        Assert.That(instance.TestDictionary["test1"], Is.EqualTo("test1"));
        Assert.That(instance.TestDictionary["test2"], Is.EqualTo("test2"));
    }

    private class SqliteServerAppSettings : BaseAppSettings<SqliteServerAppSettings>, IBaseSettings
    {
        public int TestInt { get; set; }
        public double TestDouble { get; set; }
        public string TestString { get; set; }
        public List<string> TestStringList { get; set; }
        public List<BaseSettingObject> TestObjectList { get; set; }
        public Dictionary<string, string> TestDictionary { get; set; }
    }
}