namespace Oip.Settings.Tests.Settings;

internal interface IBaseSettings
{
    string ConnectionString { get; set; }
    int TestInt { get; set; }
    double TestDouble { get; set; }
    string TestString { get; set; }
    List<string> TestStringList { get; set; }
    List<BaseSettingObject> TestObjectList { get; set; }
}

internal class BaseSettingObject : IBaseSettings
{
    public string ConnectionString { get; set; }
    public int TestInt { get; set; }
    public double TestDouble { get; set; }
    public string TestString { get; set; } = string.Empty;
    public List<string> TestStringList { get; set; }
    public List<BaseSettingObject> TestObjectList { get; set; }
}