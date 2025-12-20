using Microsoft.Extensions.Configuration;
using Oip.Settings.Contexts;

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
        using var context = new AppSettingsContext(appSettings);
        if (!appSettingsOptions.ExcludeMigration)
            context.CreateTablesIfNotExist();

        context.CreateAndSaveDefaultCommon(appSettings);
        Data = context.GetDataForSettings<TAppSettings>();
    }
}