# AppCfg.Net [![Build status](https://ci.appveyor.com/api/projects/status/8ifb08lenlmbdf0p?svg=true)](https://ci.appveyor.com/project/minhhungit/appcfg) <a href="https://www.nuget.org/packages/AppCfg.Net/"><img src="https://img.shields.io/nuget/v/AppCfg.Net.svg?style=flat" /> </a>

**Type-safe, extensible, and powerful configuration framework for .NET**

AppCfg.Net provides a clean, strongly-typed approach to application configuration with support for multiple sources, custom parsers, and priority-based loading. Perfect for .NET Framework, .NET Core, and modern .NET applications.

## Table of Contents

- [Why AppCfg.Net?](#why-appcfgnet)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Key Features](#key-features)
- [Configuration Sources](#configuration-sources)
  - [Priority-Based Configuration (Recommended)](#priority-based-configuration-recommended)
  - [Environment Variables](#environment-variables)
  - [User Secrets](#user-secrets)
  - [Migrating App.config Secrets to secrets.json](#migrating-appconfig-secrets-to-secretsjson)
  - [App.config / Web.config](#appconfig--webconfig)
  - [Custom Stores (Database, Redis, etc.)](#custom-stores)
- [Type Parsers](#type-parsers)
- [Advanced Features](#advanced-features)
- [Demo Project](#demo-project)
- [Multi-Tenancy](#multi-tenancy)
- [Contributing](#contributing)
- [License](#license)

---

## Why AppCfg.Net?

### Type Safety First
No more string keys and manual type conversion. Define your configuration as interfaces and get compile-time checking, IntelliSense support, and automatic type conversion.

```csharp
// ❌ Traditional approach - error-prone
var timeout = int.Parse(ConfigurationManager.AppSettings["RequestTimeout"] ?? "30");

// ✅ AppCfg.Net - type-safe
var settings = MyAppCfg.Get<IAppSettings>();
var timeout = settings.RequestTimeout; // Already an int!
```

### Multiple Sources, One API
Load configuration from multiple sources with automatic priority handling:
- Environment Variables (perfect for containers/cloud)
- User Secrets (keep secrets out of source control)
- App.config/Web.config (traditional .NET configuration)
- Custom sources (Database, Redis, Azure Key Vault, etc.)

### Extensible & Powerful
- **22+ built-in type parsers** for primitives, collections, enums, DateTime, Guid, and more
- **Custom type parsers** for complex types (JSON, custom formats)
- **Custom stores** for any configuration source
- **Multi-tenancy support** built-in
- **Default values** and **fallback mechanisms**

---

## Installation

Install via NuGet Package Manager:

```bash
Install-Package AppCfg.Net
```

Or via .NET CLI:

```bash
dotnet add package AppCfg.Net
```

**Supported Frameworks:**
- .NET Standard 2.0+
- .NET Framework 4.6.2+
- .NET Core 2.0+
- .NET 5, 6, 7, 8+

---

## Quick Start

### Step 1: Initialize AppCfg (One Line!)

```csharp
using AppCfg;

// Recommended: Use ChainedStore for priority-based configuration
MyAppCfg.Configure(
    envVarPrefix: "MYAPP__",
    userSecretsId: "my-application-id"
);
```

This automatically sets up:
1. **Environment Variables** (highest priority) with `MYAPP__` prefix
2. **User Secrets** (medium priority) from `secrets.json`
3. **App.config/Web.config** (fallback)

### Step 2: Define Your Settings Interface

```csharp
using AppCfg;

public interface IAppSettings
{
    [Option(Alias = "Database:ConnectionString")]
    string ConnectionString { get; }

    [Option(Alias = "Api:Timeout", DefaultValue = 30)]
    int ApiTimeout { get; }

    [Option(Alias = "Features:EnableCache")]
    bool EnableCache { get; }
}
```

### Step 3: Use Your Settings

```csharp
var settings = MyAppCfg.Get<IAppSettings>();

Console.WriteLine($"Connection: {settings.ConnectionString}");
Console.WriteLine($"Timeout: {settings.ApiTimeout} seconds");
Console.WriteLine($"Cache: {settings.EnableCache}");
```

**That's it!** After calling `Configure()`, all settings automatically use priority-based loading without any special attributes needed.

---

## Key Features

### ✅ Built-in Type Support (22+ Types)

**Primitive Types:**
- `bool`, `int`, `long`, `decimal`, `double`
- `string`, `DateTime`, `TimeSpan`, `Guid`
- Enums (by name or value)

**Collections:**
- `List<T>` with custom separators
- `IReadOnlyList<T>` for immutable collections
- Supports: int, string, bool, DateTime, Guid, decimal, double, long, TimeSpan, Enum

**Special Types:**
- `SqlConnectionStringBuilder` for connection strings
- Custom JSON types via `IJsonDataType`
- Nested interfaces for hierarchical configuration

### ✅ Multiple Configuration Sources

- **App.config / Web.config** - Traditional .NET configuration
- **Environment Variables** - Perfect for containers and cloud deployments
- **User Secrets** - Keep sensitive data out of source control
- **Priority-Based Loading** - Automatically combines all sources with `Configure()`
- **Custom Stores** - Database, Redis, Azure Key Vault, or any custom source

### ✅ Extensibility

- **Custom Type Parsers** - Handle any custom format or complex type
- **Custom Stores** - Load configuration from anywhere
- **Registration API** - Clean, fluent API for registering custom components

### ✅ Developer-Friendly

- **DefaultOption attribute** - Apply default ProfileKey to all properties
- **DefaultValue attribute** - Fallback values when configuration is missing
- **RawValue attribute** - Inline default values in attributes
- **Multi-tenancy** - Built-in support for tenant-specific configuration
- **Nested settings** - Organize complex configuration hierarchies

---

## Configuration Sources

### Priority-Based Configuration (Recommended)

Call `Configure()` once at startup, and **all your settings** automatically use priority-based loading. This is the **recommended approach** for production applications.

**Priority Order:**
1. Environment Variables (highest)
2. User Secrets
3. App.config (fallback)

**Setup:**

```csharp
// One line initialization - makes priority-based loading the default!
MyAppCfg.Configure(
    envVarPrefix: "MYAPP__",
    userSecretsId: "my-app-secrets"
);

// Define settings - no special attributes needed!
public interface ISettings
{
    [Option(Alias = "Database:Host")]
    string DatabaseHost { get; }
}

// Use settings - automatically checks all sources in priority order
var settings = MyAppCfg.Get<ISettings>();
```

**Configuration Files:**

```xml
<!-- App.config -->
<appSettings>
  <add key="Database:Host" value="localhost" />
</appSettings>
```

```json
// secrets.json (overrides App.config)
{
  "Database:Host": "dev-server"
}
```

```bash
# Environment variable (overrides everything)
export MYAPP__Database__Host=prod-server
```

**Benefits:**
- ✅ Keep secrets out of source control
- ✅ Easy environment-specific configuration
- ✅ Clear priority order
- ✅ No code changes between environments
- ✅ **Automatic** - no special attributes needed on interfaces!

---

### Environment Variables

Perfect for containerized applications and cloud deployments.

**Setup:**

```csharp
using AppCfg.SettingStore;

MyAppCfg.Configure(envVarPrefix: "MYAPP__");

public interface ISettings
{
    [Option(Alias = "Api:Key")]
    string ApiKey { get; }
}
```

**Usage:**

```bash
# Set environment variable
export MYAPP__Api__Key=my-secret-key

# Or in Docker
docker run -e MYAPP__Api__Key=my-secret-key myapp
```

**Variable Format:** `PREFIX__Section__Key` (double underscore separators)

---

### User Secrets

Keep sensitive configuration out of source control, similar to .NET Core's Secret Manager.

**Setup:**

```csharp
MyAppCfg.Configure(userSecretsId: "my-app-secrets");
```

**Secrets File Location:**
- **Windows:** `%APPDATA%\Microsoft\UserSecrets\my-app-secrets\secrets.json`
- **Linux/Mac:** `~/.microsoft/usersecrets/my-app-secrets/secrets.json`

**secrets.json Example:**

```json
{
  "Database:Password": "super-secure-password",
  "Api:SecretKey": "my-secret-api-key",
  "Encryption:Key": "encryption-key-here"
}
```

**secrets.json format:** flat `"Section:Key"` names (what `dotnet user-secrets set` writes), nested objects, and arrays (`"Hosts": ["a","b"]` becomes `Hosts:0`, `Hosts:1`) are all accepted, exactly like the .NET Core JSON provider. A `null` value counts as missing, so `DefaultValue` applies. A malformed secrets.json throws `AppCfgException` instead of being silently ignored.

---

### Migrating App.config Secrets to secrets.json

Moving from a classic .NET Framework project? Passwords and API keys usually live in `App.config` / `Web.config` and end up in source control. A full step-by-step walkthrough for both .NET Framework (`App.config` / `Web.config`) and .NET Core (`appsettings.json`) is in [docs/MIGRATION-TO-SECRETS.md](docs/MIGRATION-TO-SECRETS.md). `UserSecretsMigrator` copies them into `secrets.json` in one call, so you can delete them from the config file and switch to `Configure()` without retyping anything.

**Option 1 - one line of C# (run once, e.g. from a scratch console app or a unit test):**

```csharp
using AppCfg.SettingStore;

// Migrate the running app's App.config (appSettings + connectionStrings)
var result = UserSecretsMigrator.MigrateFromCurrentConfig(
    "my-app-secrets",
    new MigrationOptions
    {
        // Optional: only move the sensitive keys
        KeyFilter = key => key.EndsWith("Password") || key.EndsWith("Key") || key.Contains("Secret"),
        IncludeConnectionStrings = true,   // default: connection strings are migrated under their name
        OverwriteExisting = false          // default: values already in secrets.json are kept
    });

Console.WriteLine($"Wrote {result.MigratedKeys.Count} secrets to {result.SecretsFilePath}");
// result.SkippedKeys lists keys that already existed in secrets.json

// Or migrate any config file on disk (Web.config, MyApp.exe.config, ...):
UserSecretsMigrator.MigrateFromConfigFile(@"C:\src\MyApp\Web.config", "my-app-secrets");

// .NET Core appsettings.json (nested objects/arrays are flattened to "A:B" / "A:0"):
UserSecretsMigrator.MigrateFromJsonFile(@"C:\src\MyApp\appsettings.Development.json", "my-app-secrets");

// Preview without writing:
IDictionary<string, string> preview = UserSecretsMigrator.ReadConfigFile(@"C:\src\MyApp\Web.config");
```

`<appSettings file="...">`, `configSource="..."`, `<remove>` and `<clear>` are honoured. Connection strings inherited from machine.config (`LocalSqlServer`) are ignored.

**Option 2 - PowerShell, nothing to build:**

```powershell
# Scripts/Migrate-AppConfigToUserSecrets.ps1 (Windows PowerShell 5.1 or PowerShell 7+)
.\Scripts\Migrate-AppConfigToUserSecrets.ps1 -ConfigPath .\Web.config -UserSecretsId my-app-secrets `
    -Include '*Password*','*Key','*Secret*' -WhatIf     # dry run: shows what would be written

.\Scripts\Migrate-AppConfigToUserSecrets.ps1 -ConfigPath .\Web.config -UserSecretsId my-app-secrets `
    -Include '*Password*','*Key','*Secret*'             # write it
# -Exclude '*Timeout*'      skip keys
# -SkipConnectionStrings    leave <connectionStrings> alone
# -Overwrite                replace values already in secrets.json
```

**Option 3 - `appcfg-migrate` command-line tool (dotnet tool):**

```bash
# Install once (needs the .NET SDK; the tool is a small net8.0 app that runs on any newer runtime too)
dotnet tool install -g AppCfg.Net.Migrate

# Preview
appcfg-migrate .\Web.config --id my-app-secrets --dry-run

# Migrate only the sensitive keys
appcfg-migrate .\Web.config --id my-app-secrets -i "*Password*" -i "*Key" -i "*Secret*"

# .NET Core: pass a .json file instead (appsettings.json, appsettings.Development.json, ...)
appcfg-migrate .ppsettings.Development.json --id my-app-secrets -i "ConnectionStrings:*" -i "*Secret*"

# Options: -i/--include <wildcard>  -e/--exclude <wildcard>  --no-connection-strings  --overwrite  -n/--dry-run
```

Until the package is published, build it from this repo: `dotnet pack AppCfg.Migrate -c Release` then `dotnet tool install -g AppCfg.Net.Migrate --add-source AppCfg.Migrate/nupkg`.

**After migrating:**

1. Delete the migrated keys from `App.config` / `Web.config` (keep non-secret settings there).
2. Call `MyAppCfg.Configure(envVarPrefix: "MYAPP__", userSecretsId: "my-app-secrets")` at startup.
3. Your `[Option(Alias = "...")]` names do not change: `secrets.json` now overrides `App.config`, and environment variables override both.

The secrets file location is available from code via `UserSecretsStore.GetSecretsFilePath("my-app-secrets")`.

---

### App.config / Web.config

Traditional .NET configuration files.

```xml
<configuration>
  <appSettings>
    <add key="Database:Host" value="localhost" />
    <add key="Api:Timeout" value="30" />
    <add key="Features:EnableCache" value="true" />
  </appSettings>

  <connectionStrings>
    <add name="MainDb"
         connectionString="Server=localhost;Database=MyDb;..."
         providerName="System.Data.SqlClient" />
  </connectionStrings>
</configuration>
```

```csharp
public interface ISettings
{
    [Option(Alias = "Database:Host")]
    string DatabaseHost { get; }

    [Option(Alias = "MainDb")]
    SqlConnectionStringBuilder MainDb { get; }
}
```

---

### Custom Stores

Load configuration from any source: databases, Redis, Azure Key Vault, etc.

**Example: SQL Server Store**

```csharp
using AppCfg;
using System.Data.SqlClient;

public static class DatabaseStore
{
    public const string StoreKey = "DatabaseStore";

    public static void Register()
    {
        MyAppCfg.SettingStores.RegisterStore(StoreKey, opt =>
        {
            using (var conn = new SqlConnection("your-connection-string"))
            {
                conn.Open();
                var cmd = new SqlCommand(
                    "SELECT Value FROM Settings WHERE Key = @key AND Tenant = @tenant",
                    conn
                );
                cmd.Parameters.AddWithValue("@key", opt.SettingKey);
                cmd.Parameters.AddWithValue("@tenant", opt.TenantKey ?? "default");

                return cmd.ExecuteScalar() as string;
            }
        });
    }
}

// Usage
DatabaseStore.Register();

public interface ISettings
{
    [Option(Alias = "FeatureFlag", ProfileKey = DatabaseStore.StoreKey)]
    bool IsFeatureEnabled { get; }
}
```

**Example: Redis Store**

```csharp
public static class RedisStore
{
    public const string StoreKey = "RedisStore";

    public static void Register()
    {
        MyAppCfg.SettingStores.RegisterStore(StoreKey, opt =>
        {
            // Use StackExchange.Redis or similar
            var redis = ConnectionMultiplexer.Connect("localhost:6379");
            var db = redis.GetDatabase();

            var key = $"config:{opt.TenantKey}:{opt.SettingKey}";
            var value = db.StringGet(key);

            return value.HasValue ? (string)value : null;
        });
    }
}
```

---

## Type Parsers

### Built-in Type Parsers

AppCfg.Net includes 22+ built-in type parsers:

| Type | Example | Notes |
|------|---------|-------|
| `bool` | `true`, `false`, `1`, `0` | Case-insensitive |
| `int` | `42`, `1,234` | Supports thousand separators |
| `long` | `9223372036854775807` | Large integers |
| `decimal` | `123.45`, `1,234.56` | Precise decimals |
| `double` | `1.7E+3` | Scientific notation |
| `string` | Any text | Trimmed by default |
| `DateTime` | `2024-01-30`, `01/30/2024` | Customizable format |
| `TimeSpan` | `01:30:00`, `1.12:00:00` | Multiple formats |
| `Guid` | `{guid}`, `guid` | With or without braces |
| `Enum` | By name or value | Case-insensitive |
| `List<T>` | Separated values | Custom separator |
| `IReadOnlyList<T>` | Immutable collections | |
| `SqlConnectionStringBuilder` | Connection strings | Strongly-typed |

### Custom Type Parsers

Create custom parsers for complex types:

```csharp
using AppCfg.Core;

public class JsonPersonParser : ITypeParserRawBuilder
{
    public Type Type => typeof(JsonPerson);

    public object Parse(string rawValue, TypeParserSettings settings)
    {
        // Load from file
        var jsonContent = File.ReadAllText(rawValue);
        return JsonConvert.DeserializeObject<JsonPerson>(jsonContent);
    }
}

// Register the parser
MyAppCfg.TypeParsers.Register(new JsonPersonParser());

// Use in settings
public interface ISettings
{
    [Option(Alias = "person-config")]
    JsonPerson Person { get; }
}
```

---

## Advanced Features

### DefaultOption Attribute

Apply a default ProfileKey to all properties in an interface (useful with custom stores):

```csharp
[DefaultOption(ProfileKey = "MyDatabaseStore")]
public interface IDatabaseSettings
{
    // All properties will use "MyDatabaseStore" by default
    string Host { get; }
    int Port { get; }
    string Username { get; }

    // Individual properties can override the default
    [Option(ProfileKey = "DifferentStore")]
    string Password { get; }
}
```

### RawValue Attribute

Provide inline default values directly in the attribute:

```csharp
public interface ISettings
{
    // If "Numbers" key doesn't exist in config, use inline default
    [Option(Alias = "Numbers", RawValue = "1;2;3", Separator = ";")]
    List<int> Numbers { get; }
}
```

### Nested Settings

Organize complex configuration with nested interfaces:

```csharp
public interface IAppSettings
{
    [Option(Alias = "App:Name")]
    string AppName { get; }

    // Nested settings
    IDatabaseSettings Database { get; }
    IApiSettings Api { get; }
}

public interface IDatabaseSettings
{
    [Option(Alias = "Database:Host")]
    string Host { get; }
}
```

### IReadOnlyList Collections

Use immutable collections for thread-safe configuration:

```csharp
public interface ISettings
{
    [Option(Alias = "AllowedHosts", Separator = ";")]
    IReadOnlyList<string> AllowedHosts { get; }
}
```

---

## Demo Project

The **AppCfgDemoComplete** project demonstrates all features in an interactive menu-driven application.

### Running the Demo

1. Open `AppCfgSolution.sln` in Visual Studio
2. Set `AppCfgDemoComplete` as startup project
3. Run the application

### Demo Features (16 Interactive Demos)

1. **Basic Types Demo** - All primitive types and collections
2. **JSON Configuration Demo** - Complex objects from JSON
3. **Connection String Demo** - SqlConnectionStringBuilder
4. **Custom Parser Demo** - Custom type parsers with file loading
5. **⭐ Priority-Based Configuration Demo** - Automatic priority loading (RECOMMENDED)
6. **Environment Variables Demo** - Environment variable store
7. **User Secrets Demo** - User secrets store
8. **Database Store Demo** - SQL Server custom store
9. **Redis Store Demo** - Redis custom store
10. **DefaultOption Demo** - DefaultOption attribute for ProfileKey
11. **Multi-Tenancy Demo** - Tenant-specific settings
12. **Nested Settings Demo** - Hierarchical configuration
13. **Advanced Features Demo** - IReadOnlyList, RawValue inline defaults
14. **Error Handling Demo** - Missing values, type errors
15. **Computed Settings Demo** - Derive values from other settings
16. **Migration Demo** - App.config → secrets.json with `UserSecretsMigrator`

### Optional Features

To enable Database and Redis demos:
1. Set `Demo:EnableDatabase` and `Demo:EnableRedis` to `true` in `App.config`
2. Setup SQL Server / Redis according to instructions in the demo
3. Run the SQL scripts from `CustomStores/MssqlStore.cs`

---

## Multi-Tenancy

AppCfg.Net has built-in support for multi-tenant applications:

```csharp
// Define settings
public interface ITenantSettings
{
    [Option(Alias = "MaxUsers")]
    int MaxUsers { get; }
}

// Get settings for default tenant
var defaultSettings = MyAppCfg.Get<ITenantSettings>();

// Get settings for specific tenant
var tenant1Settings = MyAppCfg.Get<ITenantSettings>("tenant-1");
var tenant2Settings = MyAppCfg.Get<ITenantSettings>("tenant-2");
```

**Custom Store Support:**

Multi-tenancy works seamlessly with custom stores. The tenant key is passed to your store implementation via `opt.TenantKey`.

---

## Best Practices

### ✅ DO: Use ChainedStore for Production

```csharp
MyAppCfg.Configure(
    envVarPrefix: "MYAPP__",
    userSecretsId: "my-app-secrets"
);
```

### ✅ DO: Use DefaultOption for Custom Stores

```csharp
[DefaultOption(ProfileKey = "MyCustomStore")]
public interface ISettings { ... }
```

### ✅ DO: Provide Default Values

```csharp
[Option(Alias = "Timeout", DefaultValue = 30)]
int Timeout { get; }
```

### ✅ DO: Use Strongly-Typed Properties

```csharp
int Port { get; } // ✅ Good
string Port { get; } // ❌ Avoid - parse manually
```

### ✅ DO: Keep Secrets Out of Source Control

Use User Secrets or Environment Variables for sensitive data.

### ❌ DON'T: Access Configuration in Static Constructors

Initialize AppCfg before accessing any settings.

### ❌ DON'T: Cache Settings in Static Fields (if dynamic)

If configuration can change, call `MyAppCfg.Get<T>()` each time.

---

## Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## License

This project is licensed under the MIT License - see the LICENSE file for details.

---

## Links

- **NuGet Package:** https://www.nuget.org/packages/AppCfg.Net/
- **GitHub Repository:** https://github.com/minhhungit/AppCfg.Net
- **Issue Tracker:** https://github.com/minhhungit/AppCfg.Net/issues

---

**Built with ❤️ for .NET developers who value type safety and clean code.**
