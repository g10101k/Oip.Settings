using Microsoft.EntityFrameworkCore;
using Oip.Settings.Entities;
using Oip.Settings.EntityConfigurations;
using Oip.Settings.Helpers;

namespace Oip.Settings.Contexts;

/// <summary>
/// Database context for application settings
/// </summary>
public class AppSettingsContext : DbContext
{
    private readonly IAppSettings _appSettings;
    private readonly AppSettingsOptions _appSettingsOptions;

    /// <summary>
    /// Initializes a new instance of the AppSettingsContext
    /// </summary>
    /// <param name="appSettings">Configuration options for application settings</param>
    public AppSettingsContext(IAppSettings appSettings) : base(
        appSettings.AppSettingsOptions.Builder(appSettings.Provider, appSettings.NormalizedConnectionString).Options)
    {
        _appSettings = appSettings;
        _appSettingsOptions = appSettings.AppSettingsOptions;
    }

    /// <summary>
    /// Application settings DbSet
    /// </summary>
    public DbSet<AppSettingEntity> AppSettings => Set<AppSettingEntity>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (!Database.IsSqlite() && !Database.IsInMemory())
            modelBuilder.HasDefaultSchema(_appSettingsOptions.AppSettingsSchema);
        modelBuilder.ApplyConfiguration(new AppSettingConfiguration(_appSettingsOptions.AppSettingsTable,
            _appSettingsOptions.AppSettingsSchema));
    }

    /// <summary>
    /// Creates the database schema and tables if they don't exist
    /// </summary>
    public void CreateTablesIfNotExist()
    {
        string sqlFormat;
        string sql;
        if (Database.IsSqlite())
        {
            sqlFormat = """
                        CREATE TABLE IF NOT EXISTS {0}
                        (
                            Key   TEXT not null constraint PK_AppSetting primary key,
                            Value TEXT not null
                        );
                        """;
            sql = string.Format(sqlFormat, _appSettingsOptions.AppSettingsTable);
            Database.ExecuteSqlRaw(sql);
        }
        else if (Database.IsSqlServer())
        {
            sqlFormat = """
                        BEGIN TRY
                            BEGIN TRANSACTION
                            IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = '{0}')
                            BEGIN
                                EXEC('CREATE SCHEMA [{0}]')
                            END
                            
                            IF OBJECT_ID('[{0}].[{1}]', 'U') IS NULL
                            BEGIN
                                CREATE TABLE [{0}].[{1}]
                                (
                                    [Key] nvarchar(512) not null constraint PK_{0}_{1} primary key,
                                    [Value] nvarchar(max) not null
                                )
                            END
                            
                            COMMIT TRANSACTION
                        END TRY
                        BEGIN CATCH
                            ROLLBACK TRANSACTION
                            THROW
                        END CATCH
                        """;
            sql = string.Format(sqlFormat, _appSettingsOptions.AppSettingsSchema,
                _appSettingsOptions.AppSettingsTable);
            Database.ExecuteSqlRaw(sql);
        }
        else if (Database.IsNpgsql())
        {
            sqlFormat = """
                        CREATE SCHEMA IF NOT EXISTS "{0}";
                        CREATE TABLE IF NOT EXISTS "{0}"."{1}"
                        (
                            "Key" varchar(512) not null primary key,
                            "Value" text not null
                        );
                        """;
            Database.ExecuteSqlRaw(string.Format(sqlFormat, _appSettingsOptions.AppSettingsSchema,
                _appSettingsOptions.AppSettingsTable));
        }
    }

    internal void CreateAndSaveDefaultCommon<TAppSettings>(TAppSettings settings, bool overwrite = false)
        where TAppSettings : class, IAppSettings
    {
        if (_appSettingsOptions.UseJsonStorage)
        {
            CreateAndSaveDefaultValuesAsJson(settings, overwrite);
        }
        else
        {
            CreateAndSaveDefaultValues(settings, overwrite);
        }
    }

    /// <summary>
    /// Create and save settings as JSON to db
    /// </summary>
    private void CreateAndSaveDefaultValuesAsJson<TAppSettings>(TAppSettings settings, bool overwrite = false)
        where TAppSettings : class, IAppSettings
    {
        var typeName = typeof(TAppSettings).FullName!;
        var existingEntity = AppSettings.FirstOrDefault(x => x.Key == typeName);

        if (existingEntity == null)
        {
            var jsonValue = JsonHelper<TAppSettings>.ToJson(settings);

            AppSettings.Add(new AppSettingEntity
            {
                Key = typeName,
                Value = jsonValue,
            });
        }
        else
        {
            if (overwrite)
            {
                existingEntity.Value = JsonHelper<TAppSettings>.ToJson(settings);
            }
        }

        SaveChanges();
    }

    /// <summary>
    /// Create and save settings with default value to db (traditional key-value)
    /// </summary>
    private void CreateAndSaveDefaultValues<TAppSettings>(TAppSettings appSettings, bool overwrite = false)
        where TAppSettings : class, IAppSettings
    {
        var configValues = Flatter.ToDictionary(appSettings);
        var list = AppSettings.ToList();

        foreach (var keyValue in configValues)
        {
            if (list.Exists(x => x.Key == keyValue.Key))
            {
                if (overwrite)
                {
                    AppSettings.FirstOrDefault(x => x.Key == keyValue.Key)?.Value = keyValue.Value;
                }
            }
            else
            {
                AppSettings.Add(new AppSettingEntity
                {
                    Key = keyValue.Key,
                    Value = keyValue.Value
                });
            }
        }

        SaveChanges();
    }


    internal IDictionary<string, string> GetDataForSettings<TAppSettings>()
        where TAppSettings : class, IAppSettings
    {
        return _appSettingsOptions.UseJsonStorage
            ? LoadJsonData<TAppSettings>()
            : AppSettings.ToDictionary(c => c.Key, c => c.Value)!;
    }

    /// <summary>
    /// Load data from JSON storage
    /// </summary>
    private Dictionary<string, string> LoadJsonData<TAppSettings>()
        where TAppSettings : class, IAppSettings
    {
        var typeName = typeof(TAppSettings).FullName!;
        var jsonEntity = AppSettings.FirstOrDefault(x => x.Key == typeName);

        if (jsonEntity != null && !string.IsNullOrEmpty(jsonEntity.Value))
        {
            var deserializedSettings = JsonHelper<TAppSettings>.FromJson(jsonEntity.Value);
            if (deserializedSettings != null)
            {
                return Flatter.ToDictionary(deserializedSettings);
            }
        }
        else
        {
            return Flatter.ToDictionary(_appSettings);
        }

        return new Dictionary<string, string>();
    }

    internal void SyncSettings<TAppSettings>(TAppSettings appSettings) where TAppSettings : class, IAppSettings
    {
        CreateAndSaveDefaultCommon(appSettings, true);
    }
}