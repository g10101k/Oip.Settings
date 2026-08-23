[English](README.md) | **Русский**

# Oip.Settings

Настройки приложения с провайдером EF Core. Источники перечислены по убыванию приоритета — каждый
следующий перекрывается всеми предыдущими:

* Аргументы командной строки
* Переменные окружения
* Docker secrets
* User secrets
* `appsettings.modules.json` — конфигурация модулей, если файл существует
* `spa.proxy.json` — конфигурация SPA-прокси, если файл существует
* `appsettings.Development.json` — имя задаётся через `JsonFileNameDevelopment`
* `appsettings.json` — имя задаётся через `JsonFileName`
* EF Core — таблица настроек в БД, подключается при `UseEfCoreProvider = true`

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
AppSettings.Instance.ConnectionString.Provider;                  // XpoProvider.SQLite
AppSettings.Instance.ConnectionString.NormalizeConnectionString; // Data Source=settings.db
AppSettings.Instance.ConnectionString.ConnectionString;          // XpoProvider=SQLite;Data Source=settings.db
````

`ConnectionString` имеет тип `ConnectionModel` и является единственным местом, где хранится разобранная строка
подключения. Прежние свойства `Provider`, `NormalizedConnectionString` и `Connection` удалены,
используйте вместо них `ConnectionString.Provider` и `ConnectionString.NormalizeConnectionString`.

## Логирование чувствительных данных

`SensitiveDataLogging` — собственный параметр строки подключения, такой же как `XpoProvider`. Он вырезается из
нормализованной строки подключения и включает логирование чувствительных данных EF Core для `DbContext` настроек:

````json
{
  "ConnectionString": "XpoProvider=SQLite;SensitiveDataLogging=true;Data Source=settings.db"
}
````

````csharp
AppSettings.Instance.ConnectionString.SensitiveDataLogging;      // true
AppSettings.Instance.ConnectionString.NormalizeConnectionString; // Data Source=settings.db
````

Не включайте его в продакшене: EF Core начнёт записывать значения параметров в лог.

## Обратное приведение модели к строке

Преобразование работает и в обратную сторону: `ConnectionModel` неявно приводится к строке, поэтому его можно
передавать туда, где ожидается `string`. Возвращается исходная строка подключения ровно в том виде, в каком она
записана в конфигурации, вместе с собственными параметрами — то же значение, что и у `ToString()`:

````csharp
string raw = AppSettings.Instance.ConnectionString; // XpoProvider=SQLite;Data Source=settings.db
````

Для открытия соединения указывайте `NormalizeConnectionString` явно — неявное приведение сохраняет
`XpoProvider=` и другие собственные параметры, которые провайдер БД не поймёт:

````csharp
optionsBuilder.UseSqlite(AppSettings.Instance.ConnectionString.NormalizeConnectionString);
````

# Параметры инициализации

`Initialize` принимает либо готовый `AppSettingsOptions`, либо отдельные параметры — заданные значения
перекрывают значения по умолчанию:

````csharp
AppSettings.Initialize(
    programArguments: args,
    useEfCoreProvider: true,                                 // читать настройки из БД, по умолчанию true
    normalizeConnectionString: true,                         // вырезать собственные параметры, по умолчанию true
    jsonFileName: "appsettings.json",
    jsonFileNameDevelopment: "appsettings.Development.json",
    appSettingsTable: "AppSetting",                          // таблица настроек
    appSettingsSchema: "settings",                           // схема таблицы
    builder: (provider, connectionString) => ...);           // свой DbContextOptionsBuilder
````

````csharp
AppSettings.Initialize(new AppSettingsOptions
{
    ProgramArguments = args,
    UseJsonStorage = true,      // хранить настройки в БД одной json-строкой, по умолчанию false
    ExcludeMigration = true     // не создавать таблицу настроек из приложения
});
````

Полный список источников конфигурации и их приоритет приведены в начале справки.

При `UseJsonStorage = false` (по умолчанию) настройки хранятся в таблице как плоские пары ключ-значение,
при `UseJsonStorage = true` — одной записью, где ключ равен полному имени типа настроек, а значение — json.

# Перечитывание настроек и событие OnChange

`Rebind()` перечитывает конфигурацию в существующий экземпляр, `SaveSettingsToDb()` записывает текущие
значения в БД и следом вызывает `Rebind()`. После перепривязки срабатывает событие `OnChange`:

````csharp
AppSettings.Instance.OnChange += () =>
{
    Console.WriteLine($"Настройки обновлены: {AppSettings.Instance.TestInt}");
};

AppSettings.Instance.TestInt = 42;
AppSettings.Instance.SaveSettingsToDb();
````

Экземпляр настроек — синглтон, поэтому ссылки, полученные до перечитывания, остаются актуальными.
Изменения json-файлов подхватываются автоматически (`reloadOnChange`), но `OnChange` в этом случае
не вызывается — событие срабатывает только на явный `Rebind()`.

# Переменные окружения ASP.NET Core

Стандартные переменные окружения ASP.NET Core привязываются к свойствам базового класса:

| Свойство                 | Переменная окружения          |
|--------------------------|-------------------------------|
| `AspNetCoreEnvironment`  | `ASPNETCORE_ENVIRONMENT`      |
| `AspNetCoreUrls`         | `ASPNETCORE_URLS`             |
| `AspNetCoreHttpPorts`    | `ASPNETCORE_HTTP_PORTS`       |
| `AspNetCoreHttpsPorts`   | `ASPNETCORE_HTTPS_PORTS`      |
| `AspNetCoreContentRoot`  | `ASPNETCORE_CONTENTROOT`      |
| `AspNetCoreWebRoot`      | `ASPNETCORE_WEBROOT`          |

`IsDevelopment()` возвращает `true`, если `AspNetCoreEnvironment` равно `Development` (без учёта регистра):

````csharp
if (AppSettings.Instance.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
````

# Регистрация настроек в DI

`AddAppSettingsDbContext` регистрирует `AppSettingsContext` как scoped-сервис,
`AddSettingsToDependencyInjection` — сам экземпляр настроек как синглтон, а также все его составные свойства,
чтобы их можно было внедрять по отдельности:

````csharp
builder.Services.AddAppSettingsDbContext(AppSettings.Instance);
builder.Services.AddSettingsToDependencyInjection(AppSettings.Instance);
````

Простые типы (примитивы, `string`, перечисления, `decimal`, `DateTime`, `Guid`, массивы, `List<>`,
`Dictionary<,>`) и свойства со значением `null` в DI не регистрируются.

# Атрибуты

`[NotSaveToDb]` исключает свойство из сохранения в БД — им уже помечены `ConnectionString`,
`AppSettingsOptions` и свойства `ASPNETCORE_*`. `[NotAddToDependencyInjection]` исключает свойство
из регистрации в DI:

````csharp
public class AppSettings : BaseAppSettings<AppSettings>
{
    [NotSaveToDb]
    public string Secret { get; set; } = default!;

    [NotAddToDependencyInjection]
    public SmtpOptions Smtp { get; set; } = new();
}
````

# Запуск тестов

Запуск всех тестов

````shell
dotnet test ./src/Oip.Settings.sln
````

Запуск тестов, которым не нужны внешние сервисы

````shell
dotnet test ./src/Oip.Settings.sln --filter "TestCategory!=Integration"
````

Запуск только тестов, которым нужны внешние сервисы

````shell
dotnet test ./src/Oip.Settings.sln --filter "TestCategory=Integration"
````
