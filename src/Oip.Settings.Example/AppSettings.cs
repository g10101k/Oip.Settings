namespace Oip.Settings.Example;

public class AppSettings : BaseAppSettings<AppSettings>, ISettings
{
    public string TestString { get; set; } = "test";
    public int TestInt { get; set; } = 1;
}


internal interface ISettings : IAppSettings
{
    string TestString { get; set; }
    int TestInt { get; set; }
}