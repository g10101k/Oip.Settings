# Oip.Settings

Application settings with EFCore provider with priority:

* Command line argument
* Environment variables
* Json file
* EF Core

# Startup

1. Make class with settings

````csharp
public class AppSettings : BaseAppSettings<AppSettings>
{
    public string TestString { get; set; } = "test";
    public int TestInt { get; set; } = 1;
}
````

2. Initialize settings with ConnectionString in in json file, command line arg or other

````csharp
public class Program
{
    public static void Main(string[] args)
    {
        // Initialize settings 
        AppSettings.Initialize(args);

        var builder = WebApplication.CreateBuilder(args);
        // Add settings db context
        builder.Services.AddAppSettingsDbContext(AppSettings.Instance);
        
        var app = builder.Build();
        app.MapGet("/", () => $"AppSettings.Instance.TestInt: {AppSettings.Instance.TestInt}");

        app.Run();
    }
}
````

# Connection string as a model

`ConnectionString` is written as a plain string in `appsettings.json`, but is available in code as a
`ConnectionModel`:

````json
{
  "ConnectionString": "XpoProvider=SQLite;Data Source=settings.db"
}
````

````csharp
AppSettings.Instance.Connection.Provider;                 // XpoProvider.SQLite
AppSettings.Instance.Connection.NormalizeConnectionString; // Data Source=settings.db
AppSettings.Instance.Connection.ConnectionString;          // XpoProvider=SQLite;Data Source=settings.db
````

`ConnectionModel` has a `TypeConverter`, so any own property of this type is bound from a plain string too:

````csharp
public class AppSettings : BaseAppSettings<AppSettings>
{
    public ConnectionModel ReportConnection { get; set; } = default!;
}
````

````json
{
  "ReportConnection": "XpoProvider=Postgres;Server=localhost;Database=report;"
}
````
