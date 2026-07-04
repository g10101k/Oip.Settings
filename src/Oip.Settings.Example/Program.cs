using Oip.Settings;
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
builder.Services.AddAppSettingsDbContext(settings);

var app = builder.Build();
app.MapGet("/", () => $"AppSettings.Instance.TestInt: {settings.TestInt}");

await app.RunAsync();