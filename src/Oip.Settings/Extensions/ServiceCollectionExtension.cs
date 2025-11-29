using Microsoft.EntityFrameworkCore;
using Oip.Settings;
using Oip.Settings.Contexts;
using Oip.Settings.Enums;
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
    /// IServiceCollection extension
    /// </summary>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Adds AppSettings context and registers complex properties of <see cref="IAppSettings"/>.
        /// </summary>
        public IServiceCollection AddAppSettingsDbContext(IAppSettings appSettings)
        {
            return services.AddDbContext<AppSettingsContext>(option =>
            {
                switch (appSettings.Provider)
                {
                    case XpoProvider.SQLite:
                        option.UseSqlite(appSettings.NormalizedConnectionString);
                        break;
                    case XpoProvider.Postgres:
                        option.UseNpgsql(appSettings.NormalizedConnectionString);
                        break;
                    case XpoProvider.MSSqlServer:
                        option.UseSqlServer(appSettings.NormalizedConnectionString);
                        break;
                    case XpoProvider.InMemoryDataStore:
                        option.UseInMemoryDatabase(appSettings.NormalizedConnectionString);
                        break;
                    default:
                        throw new InvalidOperationException("Unknown provider");
                }
            });
        }

        /// <summary>
        /// Registers public object properties except simple types and properties marked with DoNotAddToDependencyInjection.
        /// </summary>
        public IServiceCollection AddSettingsToDependencyInjection(object instance)
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