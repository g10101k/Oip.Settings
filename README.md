**English** | [Русский](README.ru.md)

# Oip.Settings

Application settings with an EF Core provider. Sources are listed from the highest priority to the lowest —
every source is overridden by all the sources above it:

* Command line arguments
* Environment variables
* Docker secrets
* User secrets
* `appsettings.modules.json` — module configuration, when the file exists
* `spa.proxy.json` — SPA proxy configuration, when the file exists
* `appsettings.Development.json` — name is set with `JsonFileNameDevelopment`
* `appsettings.json` — name is set with `JsonFileName`
* EF Core — settings table in the database, added when `UseEfCoreProvider = true`

# Startup

1. Create a settings class

````csharp
public class AppSettings : BaseAppSettings<AppSettings>
{
    public string TestString { get; set; } = "test";
    public int TestInt { get; set; } = 1;
}
````

2. Initialize settings with a ConnectionString from a JSON file, a command line argument or elsewhere

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
AppSettings.Instance.ConnectionString.Provider;                  // XpoProvider.SQLite
AppSettings.Instance.ConnectionString.NormalizeConnectionString; // Data Source=settings.db
AppSettings.Instance.ConnectionString.ConnectionString;          // XpoProvider=SQLite;Data Source=settings.db
````

`ConnectionString` is typed as `ConnectionModel` and is the single place where the parsed connection string
lives. The former `Provider`, `NormalizedConnectionString` and `Connection` properties are removed, use
`ConnectionString.Provider` and `ConnectionString.NormalizeConnectionString` instead.

## Sensitive data logging

`SensitiveDataLogging` is a custom connection string parameter, just like `XpoProvider`. It is stripped from
the normalized connection string and turns on EF Core sensitive data logging for the settings `DbContext`:

````json
{
  "ConnectionString": "XpoProvider=SQLite;SensitiveDataLogging=true;Data Source=settings.db"
}
````

````csharp
AppSettings.Instance.ConnectionString.SensitiveDataLogging;      // true
AppSettings.Instance.ConnectionString.NormalizeConnectionString; // Data Source=settings.db
````

Do not enable it in production: EF Core will then write parameter values into the log.

## Converting a model back to a string

The conversion works the other way round too: `ConnectionModel` is implicitly converted to a string, so it can
be passed anywhere a `string` is expected. The result is the original connection string exactly as it is
written in configuration, custom parameters included — the same value `ToString()` returns:

````csharp
string raw = AppSettings.Instance.ConnectionString; // XpoProvider=SQLite;Data Source=settings.db
````

To open a connection use `NormalizeConnectionString` explicitly — the implicit conversion keeps
`XpoProvider=` and other custom parameters, which a database provider will not understand:

````csharp
optionsBuilder.UseSqlite(AppSettings.Instance.ConnectionString.NormalizeConnectionString);
````

# Initialization options

`Initialize` takes either a ready `AppSettingsOptions` or separate parameters — the values passed override
the defaults:

````csharp
AppSettings.Initialize(
    programArguments: args,
    useEfCoreProvider: true,                                 // read settings from the database, true by default
    normalizeConnectionString: true,                         // strip custom parameters, true by default
    jsonFileName: "appsettings.json",
    jsonFileNameDevelopment: "appsettings.Development.json",
    appSettingsTable: "AppSetting",                          // settings table
    appSettingsSchema: "settings",                           // table schema
    builder: (provider, connectionString) => ...);           // own DbContextOptionsBuilder
````

````csharp
AppSettings.Initialize(new AppSettingsOptions
{
    ProgramArguments = args,
    UseJsonStorage = true,      // store settings in the database as a single json string, false by default
    ExcludeMigration = true     // do not create the settings table from the application
});
````

The full list of configuration sources and their priority is at the top of this readme.

With `UseJsonStorage = false` (the default) settings are stored in the table as flat key-value pairs,
with `UseJsonStorage = true` — as a single row where the key is the full name of the settings type and the
value is json.

# Rebinding settings and the OnChange event

`Rebind()` rereads the configuration into the existing instance, `SaveSettingsToDb()` writes the current
values to the database and then calls `Rebind()`. The `OnChange` event is raised after rebinding:

````csharp
AppSettings.Instance.OnChange += () =>
{
    Console.WriteLine($"Settings updated: {AppSettings.Instance.TestInt}");
};

AppSettings.Instance.TestInt = 42;
AppSettings.Instance.SaveSettingsToDb();
````

The settings instance is a singleton, so the references taken before rebinding stay up to date.
Changes in json files are picked up automatically (`reloadOnChange`), but `OnChange` is not raised in that
case — the event is raised on an explicit `Rebind()` only.

# ASP.NET Core environment variables

Standard ASP.NET Core environment variables are bound to the properties of the base class:

| Property                 | Environment variable          |
|--------------------------|-------------------------------|
| `AspNetCoreEnvironment`  | `ASPNETCORE_ENVIRONMENT`      |
| `AspNetCoreUrls`         | `ASPNETCORE_URLS`             |
| `AspNetCoreHttpPorts`    | `ASPNETCORE_HTTP_PORTS`       |
| `AspNetCoreHttpsPorts`   | `ASPNETCORE_HTTPS_PORTS`      |
| `AspNetCoreContentRoot`  | `ASPNETCORE_CONTENTROOT`      |
| `AspNetCoreWebRoot`      | `ASPNETCORE_WEBROOT`          |

`IsDevelopment()` returns `true` when `AspNetCoreEnvironment` equals `Development`, ignoring case:

````csharp
if (AppSettings.Instance.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
````

# Registering settings in DI

`AddAppSettingsDbContext` registers `AppSettingsContext` as a scoped service,
`AddSettingsToDependencyInjection` registers the settings instance itself as a singleton together with all its
complex properties, so that they can be injected separately:

````csharp
builder.Services.AddAppSettingsDbContext(AppSettings.Instance);
builder.Services.AddSettingsToDependencyInjection(AppSettings.Instance);
````

Simple types (primitives, `string`, enums, `decimal`, `DateTime`, `Guid`, arrays, `List<>`, `Dictionary<,>`)
and properties with a `null` value are not registered in DI.

# Attributes

`[NotSaveToDb]` excludes a property from being saved to the database — `ConnectionString`,
`AppSettingsOptions` and the `ASPNETCORE_*` properties are already marked with it.
`[NotAddToDependencyInjection]` excludes a property from being registered in DI:

````csharp
public class AppSettings : BaseAppSettings<AppSettings>
{
    [NotSaveToDb]
    public string Secret { get; set; } = default!;

    [NotAddToDependencyInjection]
    public SmtpOptions Smtp { get; set; } = new();
}
````

# Running tests

Run all tests

````shell
dotnet test ./src/Oip.Settings.sln
````

Run tests that don't need external services

````shell
dotnet test ./src/Oip.Settings.sln --filter "TestCategory!=Integration"
````

Run only tests that need external services

````shell
dotnet test ./src/Oip.Settings.sln --filter "TestCategory=Integration"
````
