using Microsoft.Extensions.Configuration;
using Oip.Settings.Contexts;
using Oip.Settings.Entities;
using Oip.Settings.Helpers;

namespace Oip.Settings.Providers;

/// <summary>
/// EF Core settings provider
/// </summary>
/// <typeparam name="TAppSettings"></typeparam>
public class EfConfigurationProvider<TAppSettings>(AppSettingsOptions appSettingsOptions, TAppSettings appSettings)
    : ConfigurationProvider where TAppSettings : class, IAppSettings
{
    /// <summary>
    /// Load settings
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public override void Load()
    {
        var builder = appSettingsOptions.Builder(appSettings.Provider, appSettings.NormalizedConnectionString);
        using var context = new AppSettingsContext(builder.Options, appSettingsOptions);
        MigrateAndFillData(context);
    }

    private void MigrateAndFillData(AppSettingsContext context)
    {
        if (!appSettingsOptions.ExcludeMigration)
            context.CreateTablesIfNotExist();
        CreateAndSaveDefaultValues(context);
        Data = context.AppSettings.ToDictionary(c => c.Key, c => c.Value)!;
    }

    /// <summary>
    /// Create and save settings with default value to db
    /// </summary>
    /// <param name="dbContext"></param>
    private void CreateAndSaveDefaultValues(AppSettingsContext dbContext)
    {
        var configValues = Flatter.ToDictionary(appSettings);
        var list = dbContext.AppSettings.ToList();

        foreach (var keyValue in configValues)
        {
            if (list.Exists(x => x.Key == keyValue.Key))
                continue;
            dbContext.AppSettings.Add(new AppSettingEntity
            {
                Key = keyValue.Key,
                Value = keyValue.Value
            });
        }

        dbContext.SaveChanges();
    }
}