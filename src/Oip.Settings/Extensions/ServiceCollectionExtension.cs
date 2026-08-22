using Oip.Settings;
using Oip.Settings.Contexts;
using System.Reflection;
using Oip.Settings.Attributes;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// IServiceCollection extension
/// </summary>
public static class ServiceCollectionExtension
{
    /// <summary>
    /// Registers <see cref="AppSettingsContext"/> as a scoped service.
    /// The context is configured from <see cref="AppSettingsOptions.Builder"/>,
    /// so the provider and connection string come from <paramref name="appSettings"/>.
    /// </summary>
    public static IServiceCollection AddAppSettingsDbContext(this IServiceCollection services, IAppSettings appSettings)
    {
        ArgumentNullException.ThrowIfNull(appSettings);

        services.AddScoped(_ => new AppSettingsContext(appSettings));

        return services;
    }

    /// <summary>
    /// Registers the settings instance itself as a singleton under <typeparamref name="TAppSettings"/>
    /// and then registers its complex properties.
    /// </summary>
    public static IServiceCollection AddSettingsToDependencyInjection<TAppSettings>(this IServiceCollection services, TAppSettings instance)
        where TAppSettings : class, IAppSettings
    {
        if (instance == null)
            throw new ArgumentNullException(nameof(instance));

        services.AddSingleton(instance);

        return services.AddSettingsToDependencyInjection((object)instance);
    }

    /// <summary>
    /// Registers public object properties except simple types and properties marked with DoNotAddToDependencyInjection.
    /// </summary>
    public static IServiceCollection AddSettingsToDependencyInjection(this IServiceCollection services, object instance)
    {
        if (instance == null)
            throw new ArgumentNullException(nameof(instance));

        var props = instance.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in props)
        {
            if (!prop.CanRead)
                continue;

            // Skip properties marked with attribute
            if (prop.GetCustomAttribute<NotAddToDependencyInjectionAttribute>() != null)
                continue;

            var value = prop.GetValue(instance);
            if (value == null)
                continue;

            if (IsSimpleType(prop.PropertyType))
                continue;

            services.AddSingleton(prop.PropertyType, value);
        }

        return services;
    }

    /// <summary>
    /// Determines whether a type is primitive, string, enum, nullable primitive, array, list, dictionary or enumerable.
    /// </summary>
    private static bool IsSimpleType(Type type)
    {
        if (type.IsPrimitive ||
            type.IsEnum ||
            type == typeof(string) ||
            type == typeof(decimal) ||
            type == typeof(DateTime) ||
            type == typeof(Guid))
            return true;

        if (Nullable.GetUnderlyingType(type) is { } underlying && IsSimpleType(underlying))
            return true;

        if (type.IsArray)
            return true;

        if (type.IsGenericType)
        {
            var genericDef = type.GetGenericTypeDefinition();

            if (genericDef == typeof(List<>) ||
                genericDef == typeof(Dictionary<,>) ||
                typeof(IEnumerable<>).IsAssignableFrom(genericDef))
                return true;
        }

        return false;
    }
}