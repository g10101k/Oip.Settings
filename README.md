# Oip.Settings

Application settings with an EF Core provider. Sources in order of priority:

* Command line argument
* Environment variables
* JSON file
* EF Core

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
