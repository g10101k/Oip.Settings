using Microsoft.Extensions.Configuration;
using Oip.Settings.Contexts;
using Oip.Settings.Entities;
using Oip.Settings.Helpers;
using System.Text.Json;

namespace Oip.Settings.Providers;

/// <summary>
/// EF Core settings provider
/// </summary>
/// <typeparam name="TAppSettings"></typeparam>
public class EfConfigurationProvider<TAppSettings>(AppSettingsOptions appSettingsOptions, TAppSettings appSettings)
    : ConfigurationProvider where TAppSettings : class, IAppSettings
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new AppSettingsJsonConverter<TAppSettings>() }
    };

    /// <summary>
    /// Load settings
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public override void Load()
    {
        using var context = new AppSettingsContext(appSettings);
        MigrateAndFillData(context);
    }

    private void MigrateAndFillData(AppSettingsContext context)
    {
        if (!appSettingsOptions.ExcludeMigration)
            context.CreateTablesIfNotExist();

        if (appSettingsOptions.UseJsonStorage)
        {
            CreateAndSaveDefaultValuesAsJson(context);
            LoadJsonData(context);
        }
        else
        {
            CreateAndSaveDefaultValues(context);
            Data = context.AppSettings.ToDictionary(c => c.Key, c => c.Value)!;
        }
    }

    /// <summary>
    /// Create and save settings with default value to db (traditional key-value)
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

    /// <summary>
    /// Create and save settings as JSON to db
    /// </summary>
    /// <param name="dbContext"></param>
    private void CreateAndSaveDefaultValuesAsJson(AppSettingsContext dbContext)
    {
        var typeName = typeof(TAppSettings).FullName!;
        var existingEntity = dbContext.AppSettings.FirstOrDefault(x => x.Key == typeName);

        if (existingEntity == null)
        {
            var jsonValue = JsonSerializer.Serialize(appSettings, _jsonOptions);

            dbContext.AppSettings.Add(new AppSettingEntity
            {
                Key = typeName,
                Value = jsonValue,
            });

            dbContext.SaveChanges();
        }
    }

    /// <summary>
    /// Load data from JSON storage
    /// </summary>
    /// <param name="context"></param>
    private void LoadJsonData(AppSettingsContext context)
    {
        var typeName = typeof(TAppSettings).FullName!;
        var jsonEntity = context.AppSettings.FirstOrDefault(x => x.Key == typeName);

        if (jsonEntity != null && !string.IsNullOrEmpty(jsonEntity.Value))
        {
            var deserializedSettings = JsonSerializer.Deserialize<TAppSettings>(jsonEntity.Value, _jsonOptions);
            if (deserializedSettings != null)
            {
                Data = Flatter.ToDictionary(deserializedSettings);
            }
        }
        else
        {
            // Если JSON не найден, используем значения по умолчанию
            Data = Flatter.ToDictionary(appSettings);
        }
    }
}