using Oip.Settings;
using Oip.Settings.Contexts;
using Oip.Settings.Example;

var settings = AppSettings.Initialize(new AppSettingsOptions
{
    ProgramArguments = args,
    AppSettingsTable = "TestSettingsTableName",
});

if (settings.IsDevelopment())
{
    Console.WriteLine("Development mode");
}

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSettingsToDependencyInjection(settings);
builder.Services.AddAppSettingsDbContext(settings);

var app = builder.Build();
app.MapGet("/", () => $"AppSettings.Instance.TestInt: {settings.TestInt}");
app.MapGet("/count", (AppSettingsContext db) => db.AppSettings.Count());

await app.RunAsync();