using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Oip.Settings.Attributes;
using Oip.Settings.Contexts;
using Oip.Settings.Enums;
using Oip.Settings.Helpers;
using Oip.Settings.Providers;

// ReSharper disable PossibleMultipleWriteAccessInDoubleCheckLocking
// ReSharper disable ReadAccessInDoubleCheckLocking
// ReSharper disable StaticMemberInGenericType
namespace Oip.Settings;

/// <summary>
/// Base application settings class with singleton pattern and configuration binding
/// </summary>
/// <typeparam name="TAppSettings">The application settings type</typeparam>
public class BaseAppSettings<TAppSettings> : IAppSettings where TAppSettings : class, IAppSettings
{
    private static readonly object LockObject = new();

    /// <summary>
    /// Singleton instance of application settings.
    /// </summary>
    private static TAppSettings? _instance;

    /// <summary>
    /// Временный инстанс без настроек из бд
    /// </summary>
    private static TAppSettings? _temporaryInstance;

    /// <summary>
    /// Настройка поведение получения конфигурации
    /// </summary>
    private static AppSettingsOptions? _appSettingsOptions;

    /// <summary>
    /// Singleton application settings instance
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when instance creation fails</exception>
    public static TAppSettings Instance
    {
        get
        {
            if (_instance != null)
                return _instance;

            lock (LockObject)
            {
                if (_instance != null)
                    return _instance;

                _instance = CreateInstance<TAppSettings>();
                _temporaryInstance = CreateInstance<TAppSettings>();

                BindTemporaryConfiguration(_temporaryInstance);
                BindMainConfiguration(_instance, _temporaryInstance);

                return _instance;
            }
        }
    }

    /// <inheritdoc />
    [NotSaveToDb]
    public string ConnectionString { get; set; } = null!;

    /// <inheritdoc />
    [NotSaveToDb]
    public string NormalizedConnectionString { get; set; } = null!;

    /// <inheritdoc />
    [NotSaveToDb]
    public XpoProvider Provider { get; set; } = XpoProvider.InMemoryDataStore;

    /// <inheritdoc />
    [NotSaveToDb]
    public AppSettingsOptions AppSettingsOptions => _appSettingsOptions ??
                                                    throw new InvalidOperationException(
                                                        "AppSettingsOptions is not initialized. Call Initialize method first.");

    /// <summary>
    /// Initialize app settings with options
    /// </summary>
    /// <param name="appSettingsOptions">Application settings options</param>
    /// <returns>Initialized application settings instance</returns>
    public static TAppSettings Initialize(AppSettingsOptions appSettingsOptions)
    {
        _appSettingsOptions = appSettingsOptions ?? throw new ArgumentNullException(nameof(appSettingsOptions));
        return Instance;
    }

    /// <summary>
    /// Rebind main configuration from temporary instance
    /// </summary>
    public void Rebind()
    {
        if (_instance == null || _temporaryInstance == null)
            throw new InvalidOperationException("Instances are not initialized. Call Initialize method first.");

        BindMainConfiguration(_instance, _temporaryInstance);
    }

    /// <summary>
    /// Gets the application settings context
    /// </summary>
    /// <returns>The application settings context</returns>
    public static AppSettingsContext GetAppSettingsContext()
    {
        return new AppSettingsContext(Instance);
    }

    /// <summary>
    /// Initialize app settings with detailed parameters
    /// </summary>
    /// <param name="programArguments">Command line arguments</param>
    /// <param name="useEfCoreProvider">Whether to use EF Core provider</param>
    /// <param name="normalizeConnectionString">Whether to normalize connection string</param>
    /// <param name="jsonFileName">Main JSON configuration file name</param>
    /// <param name="jsonFileNameDevelopment">Development JSON configuration file name</param>
    /// <param name="appSettingsTable">Database table name for app settings</param>
    /// <param name="appSettingsSchema">Database schema name for app settings</param>
    /// <param name="builder">DbContext options builder</param>
    /// <returns>Initialized application settings instance</returns>
    public static TAppSettings Initialize(
        string[]? programArguments = null,
        bool? useEfCoreProvider = null,
        bool? normalizeConnectionString = null,
        string? jsonFileName = null,
        string? jsonFileNameDevelopment = null,
        string? appSettingsTable = null,
        string? appSettingsSchema = null,
        Func<XpoProvider, string, DbContextOptionsBuilder<AppSettingsContext>>? builder = null)
    {
        _appSettingsOptions = new AppSettingsOptions();

        SetIfNotNull(jsonFileName, value => _appSettingsOptions.JsonFileName = value);
        SetIfNotNull(jsonFileNameDevelopment, value => _appSettingsOptions.JsonFileNameDevelopment = value);
        SetIfNotNull(programArguments, value => _appSettingsOptions.ProgramArguments = value);
        SetIfNotNull(useEfCoreProvider, value => _appSettingsOptions.UseEfCoreProvider = value.Value);
        SetIfNotNull(appSettingsTable, value => _appSettingsOptions.AppSettingsTable = value);
        SetIfNotNull(appSettingsSchema, value => _appSettingsOptions.AppSettingsSchema = value);
        SetIfNotNull(builder, value => _appSettingsOptions.Builder = value);
        SetIfNotNull(normalizeConnectionString, value => _appSettingsOptions.NormalizeConnectionString = value.Value);

        return Instance;
    }

    private static void SetIfNotNull<T>(T? value, Action<T> setter)
    {
        if (value != null) setter(value);
    }

    private static T CreateInstance<T>() where T : class
    {
        return Activator.CreateInstance(typeof(T)) as T ??
               throw new InvalidOperationException($"Failed to create instance of type {typeof(T).Name}");
    }

    private static void BindTemporaryConfiguration(TAppSettings temporaryInstance)
    {
        var configuration = BuildBaseConfiguration(new ConfigurationBuilder());
        BindConfiguration(configuration, temporaryInstance);
    }

    internal static void BindMainConfiguration(TAppSettings instance, TAppSettings temporaryInstance)
    {
        if (temporaryInstance.AppSettingsOptions == null)
        {
            throw new InvalidOperationException(
                $"{nameof(_appSettingsOptions)} is null, call {nameof(Initialize)} before using {nameof(Instance)}");
        }

        var configurationBuilder = new ConfigurationBuilder();

        if (temporaryInstance.AppSettingsOptions.UseEfCoreProvider &&
            !string.IsNullOrEmpty(temporaryInstance.ConnectionString))
        {
            var efConfigurationSource = new EfConfigurationSource<TAppSettings>(
                temporaryInstance.AppSettingsOptions, temporaryInstance);
            configurationBuilder.Add(efConfigurationSource);
        }

        var configuration = BuildBaseConfiguration(configurationBuilder);

        BindConfiguration(configuration, instance);

        ChangeToken.OnChange(
            () => configuration.GetReloadToken(),
            () => BindConfiguration(configuration, instance));
    }

    internal static IConfigurationRoot BuildBaseConfiguration(ConfigurationBuilder configurationBuilder)
    {
        if (_appSettingsOptions == null)
        {
                throw new InvalidOperationException("AppSettingsOptions is not initialized");
        }

        return configurationBuilder
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(_appSettingsOptions.JsonFileName, optional: true, reloadOnChange: true)
            .AddJsonFile(_appSettingsOptions.JsonFileNameDevelopment, optional: true, reloadOnChange: true)
            .AddUserSecrets<TAppSettings>()
            .AddSpaConfig()
            .AddModuleConfig()
            .AddEnvironmentVariables()
            .AddCommandLine(_appSettingsOptions.ProgramArguments)
            .Build();
    }

    internal static void BindConfiguration(IConfiguration configuration, TAppSettings instance)
    {
        configuration.Bind(instance);
        NormalizeConnectionString(instance);
    }

    internal static void NormalizeConnectionString(TAppSettings instance)
    {
        if (string.IsNullOrEmpty(instance.ConnectionString) ||
            !instance.AppSettingsOptions.NormalizeConnectionString)
        {
            return;
        }

        var connectionModel = ConnectionStringHelper.NormalizeConnectionString(instance.ConnectionString);
        instance.NormalizedConnectionString = connectionModel.NormalizeConnectionString;
        instance.Provider = connectionModel.Provider;
    }
}