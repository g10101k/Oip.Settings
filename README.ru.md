[English](README.md) | **Русский**

# Oip.Settings

Настройки приложения с провайдером EF Core и следующим приоритетом:

* Аргумент командной строки
* Переменные окружения
* Json-файл
* EF Core

# Начало работы

1. Создайте класс с настройками

````csharp
public class AppSettings : BaseAppSettings<AppSettings>
{
    public string TestString { get; set; } = "test";
    public int TestInt { get; set; } = 1;
}
````

2. Инициализируйте настройки, задав ConnectionString в json-файле, аргументе командной строки или иным способом

````csharp
public class Program
{
    public static void Main(string[] args)
    {
        // Инициализация настроек
        AppSettings.Initialize(args);

        var builder = WebApplication.CreateBuilder(args);
        // Добавление контекста БД настроек
        builder.Services.AddAppSettingsDbContext(AppSettings.Instance);
        
        var app = builder.Build();
        app.MapGet("/", () => $"AppSettings.Instance.TestInt: {AppSettings.Instance.TestInt}");

        app.Run();
    }
}
````

# Строка подключения как модель

`ConnectionString` записывается в `appsettings.json` обычной строкой, но в коде доступна как
`ConnectionModel`:

````json
{
  "ConnectionString": "XpoProvider=SQLite;Data Source=settings.db"
}
````

````csharp
AppSettings.Instance.Connection.Provider;                  // XpoProvider.SQLite
AppSettings.Instance.Connection.NormalizeConnectionString; // Data Source=settings.db
AppSettings.Instance.Connection.ConnectionString;          // XpoProvider=SQLite;Data Source=settings.db
````

`Connection` — единственное место, где хранится разобранная строка подключения. Прежние свойства верхнего уровня
`AppSettings.Instance.Provider` и `AppSettings.Instance.NormalizedConnectionString` удалены,
используйте вместо них `Connection.Provider` и `Connection.NormalizeConnectionString`.

У `ConnectionModel` есть `TypeConverter`, поэтому любое ваше собственное свойство этого типа тоже
привязывается из обычной строки:

````csharp
public class AppSettings : BaseAppSettings<AppSettings>
{
    public ConnectionModel ReportConnection { get; set; } = default!;
}
````

````json
{
  "ReportConnection": "XpoProvider=Postgres;Server=localhost;Database=report;"
}
````

# Логирование чувствительных данных

`SensitiveDataLogging` — собственный параметр строки подключения, такой же как `XpoProvider`. Он вырезается из
нормализованной строки подключения и включает логирование чувствительных данных EF Core для `DbContext` настроек:

````json
{
  "ConnectionString": "XpoProvider=SQLite;SensitiveDataLogging=true;Data Source=settings.db"
}
````

````csharp
AppSettings.Instance.Connection.SensitiveDataLogging;      // true
AppSettings.Instance.Connection.NormalizeConnectionString; // Data Source=settings.db
````

Не включайте его в продакшене: EF Core начнёт записывать значения параметров в лог.
