# AppCfg.Net [![Build status](https://ci.appveyor.com/api/projects/status/8ifb08lenlmbdf0p?svg=true)](https://ci.appveyor.com/project/minhhungit/appcfg) <a href="https://www.nuget.org/packages/AppCfg.Net/"><img src="https://img.shields.io/nuget/v/AppCfg.Net.svg?style=flat" /> </a>

**Type-safe, easy and powerful configuration framework for .NET developers**

## Table of Contents

- [Installation](#installation)
- [Quick Start](#quick-start)
- [Why AppCfg.Net?](#why-appcfgnet)
- [Configuration](#configuration)
- [Supported Types](#supported-types)
- [Advanced Features](#advanced-features)
- [Best Practices](#best-practices)
- [Examples](#examples)
- [Troubleshooting](#troubleshooting)

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
- .NET Standard 2.0
- .NET Framework 4.6.2+
- .NET Core 2.0+
- .NET 5+

---

## Quick Start

### Step 1: Configure AppCfg (One Line)

```csharp
using AppCfg;

public class Startup
{
    public static void Init()
    {
        // One line - handles everything automatically!
        MyAppCfg.Configure(
            envVarPrefix: "APPCFG__",
            userSecretsId: "my-app-secrets"
        );
    }
}
```

### Step 2: Define Your Settings Interface

```csharp
[DefaultOption(StoreType = SettingStoreType.Custom,
               StoreIdentity = MyAppCfg.DefaultStoreIdentity)]
public interface IAppSettings
{
    [Option(Alias = "ApiKey", DefaultValue = "")]
    string ApiKey { get; }

    [Option(Alias = "Database:Host", DefaultValue = "localhost")]
    string DatabaseHost { get; }

    [Option(Alias = "Database:Port", DefaultValue = 5432)]
    int DatabasePort { get; }
}
```

### Step 3: Load and Use Settings

```csharp
Startup.Init();
var settings = MyAppCfg.Get<IAppSettings>();

Console.WriteLine($"API Key: {settings.ApiKey}");
Console.WriteLine($"Database: {settings.DatabaseHost}:{settings.DatabasePort}");
```

**That's it!** AppCfg.Net automatically checks:
1. Environment Variables (APPCFG__ prefix) - **Highest Priority**
2. User Secrets (secrets.json) - **Medium Priority**
3. AppSettings (app.config) - **Fallback**

---

## Why AppCfg.Net?

### Type Safety
No more string keys and manual type conversion. Get compile-time checking and IntelliSense support.

```csharp
// Type-safe, compile-time checked
var port = settings.DatabasePort;  // Already an int!
var host = settings.DatabaseHost;  // Already a string!
```

### Read-Only Configuration
Settings cannot be modified at runtime, preventing accidental configuration changes.

### Automatic Priority-Based Loading
Environment variables override secrets, secrets override app settings. Perfect for Docker/Kubernetes deployments.

### One Line Setup
`MyAppCfg.Configure()` handles everything - environment variables, user secrets, and app settings.

### No Repetition
Use `DefaultOption` attribute to configure once at the interface level.

### Multi-Tenancy Support
Different tenants can have different configuration values using custom stores.

---

## Configuration

### Priority Order

AppCfg.Net checks configuration sources in this order:

```
1. Environment Variables (Highest Priority) ← Docker/Kubernetes overrides
   ↓ (if not found, check next)
2. User Secrets                             ← Local development secrets
   ↓ (if not found, check next)
3. AppSettings (Fallback)                   ← Default values
```

### Setup

```csharp
using AppCfg;

MyAppCfg.Configure(
    envVarPrefix: "APPCFG__",        // Optional, defaults to "APPCFG__"
    userSecretsId: "my-app-secrets"  // Optional, null = skip user secrets
);
```

**Parameters:**
- `envVarPrefix`: Prefix for environment variables (default: `"APPCFG__"`)
- `userSecretsId`: User secrets directory name (optional)

### Configure Your Values

#### Option A: Environment Variables (Production)

```bash
# Linux/macOS
export APPCFG__ApiKey="prod-api-key"
export APPCFG__Database__Host="prod-db.example.com"
export APPCFG__Database__Port="5432"

# Windows PowerShell
$env:APPCFG__ApiKey="prod-api-key"
$env:APPCFG__Database__Host="prod-db.example.com"
$env:APPCFG__Database__Port="5432"

# Docker/Kubernetes
env:
  - name: APPCFG__ApiKey
    value: "prod-api-key"
  - name: APPCFG__Database__Host
    value: "prod-db.example.com"
```

**Note:** Use double underscore `__` for hierarchy (e.g., `Database:Port` → `APPCFG__Database__Port`)

#### Option B: User Secrets (Development)

**Create secrets file:**

**Linux/macOS:** `~/.microsoft/usersecrets/my-app-secrets/secrets.json`
**Windows:** `%APPDATA%\Microsoft\UserSecrets\my-app-secrets\secrets.json`

```json
{
  "ApiKey": "dev-api-key",
  "Database": {
    "Host": "localhost",
    "Port": 5432,
    "Password": "dev-password"
  }
}
```

**Create directory and file:**
```bash
# Linux/macOS
mkdir -p ~/.microsoft/usersecrets/my-app-secrets
nano ~/.microsoft/usersecrets/my-app-secrets/secrets.json

# Windows PowerShell
$path = "$env:APPDATA\Microsoft\UserSecrets\my-app-secrets"
New-Item -ItemType Directory -Force -Path $path
notepad "$path\secrets.json"
```

#### Option C: AppSettings (Defaults)

```xml
<appSettings>
  <add key="ApiKey" value="default-key"/>
  <add key="Database:Host" value="localhost"/>
  <add key="Database:Port" value="5432"/>
</appSettings>
```

### Real-World Scenarios

#### Scenario 1: Local Development
- Secrets in `secrets.json`
- No environment variables
- Defaults in `app.config`

**Result:** All values come from `secrets.json`, missing values fall back to `app.config`

#### Scenario 2: Docker/Kubernetes Production
- All values as environment variables (from ConfigMap/Secrets)
- No `secrets.json` in container

**Result:** All values from environment variables, overrides everything

#### Scenario 3: Mixed Override
- `Database:Password` in environment variable (for security)
- Other values in `secrets.json`

**Result:**
- `Database:Password` → from env var (highest priority)
- `ApiKey`, `Database:Host` → from `secrets.json`

---

## Supported Types

AppCfg.Net supports automatic type parsing for:

| Type | List Support | Example |
|------|--------------|---------|
| `string` | `List<string>` | `"Hello World"` |
| `int` | `List<int>` | `42` |
| `long` | `List<long>` | `9223372036854775807` |
| `bool` | `List<bool>` | `true` |
| `decimal` | `List<decimal>` | `123.45` |
| `double` | `List<double>` | `3.14159` |
| `Guid` | `List<Guid>` | `550e8400-e29b-41d4-a716-446655440000` |
| `DateTime` | `List<DateTime>` | `2024-01-29` |
| `TimeSpan` | `List<TimeSpan>` | `01:30:00` |
| `Enum` | `List<Enum>` | `MyEnum.Value1` |
| JSON Objects | - | Complex nested objects |
| ConnectionString | - | Database connection strings |

### Example:

```csharp
public interface ISettings
{
    [Option(Alias = "AppName")]
    string AppName { get; }

    [Option(Alias = "MaxRetries")]
    int MaxRetries { get; }

    [Option(Alias = "Timeout")]
    TimeSpan Timeout { get; }

    [Option(Alias = "IsEnabled")]
    bool IsEnabled { get; }

    [Option(Alias = "ClientId")]
    Guid ClientId { get; }

    [Option(Alias = "AllowedHosts")]
    List<string> AllowedHosts { get; }
}
```

**Configuration:**
```xml
<appSettings>
  <add key="AppName" value="MyApp"/>
  <add key="MaxRetries" value="3"/>
  <add key="Timeout" value="00:05:00"/>
  <add key="IsEnabled" value="true"/>
  <add key="ClientId" value="550e8400-e29b-41d4-a716-446655440000"/>
  <add key="AllowedHosts" value="localhost,example.com,test.com"/>
</appSettings>
```

---

## Advanced Features

### DefaultOption Attribute

Set configuration once at the interface level instead of on every property:

```csharp
[DefaultOption(StoreType = SettingStoreType.Custom,
               StoreIdentity = MyAppCfg.DefaultStoreIdentity)]
public interface ISettings
{
    // All properties automatically use the configured stores
    [Option(Alias = "ApiKey")]
    string ApiKey { get; }

    [Option(Alias = "Password")]
    string Password { get; }

    [Option(Alias = "ClientId")]
    Guid ClientId { get; }
}
```

**Override when needed:**
```csharp
[DefaultOption(StoreType = SettingStoreType.Custom,
               StoreIdentity = MyAppCfg.DefaultStoreIdentity)]
public interface ISettings
{
    // Uses configured stores
    [Option(Alias = "ApiKey")]
    string ApiKey { get; }

    // Override: Use AppSettings instead
    [Option(Alias = "LegacyValue", StoreIdentity = "")]
    string LegacyValue { get; }
}
```

### Nested Configuration

Organize related settings using nested interfaces:

```csharp
public interface IDatabaseSettings
{
    [Option(Alias = "Database:Host")]
    string Host { get; }

    [Option(Alias = "Database:Port")]
    int Port { get; }

    [Option(Alias = "Database:Username")]
    string Username { get; }

    [Option(Alias = "Database:Password")]
    string Password { get; }
}

[DefaultOption(StoreType = SettingStoreType.Custom,
               StoreIdentity = MyAppCfg.DefaultStoreIdentity)]
public interface IAppSettings
{
    [Option(Alias = "AppName")]
    string AppName { get; }

    // Nested settings
    IDatabaseSettings Database { get; }
}
```

### Default Values

Provide fallback values when configuration is missing:

```csharp
public interface ISettings
{
    [Option(Alias = "MaxRetries", DefaultValue = 3)]
    int MaxRetries { get; }

    [Option(Alias = "Timeout", DefaultValue = "00:05:00")]
    TimeSpan Timeout { get; }

    [Option(Alias = "IsEnabled", DefaultValue = true)]
    bool IsEnabled { get; }

    [Option(Alias = "ApiKey", DefaultValue = "")]
    string ApiKey { get; }
}
```

### JSON Configuration

Store complex objects as JSON:

```csharp
public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    public string Email { get; set; }
}

[DefaultOption(StoreType = SettingStoreType.Custom,
               StoreIdentity = MyAppCfg.DefaultStoreIdentity)]
public interface ISettings
{
    [Option(Alias = "PersonData")]
    Person PersonData { get; }
}
```

**Configuration:**
```json
{
  "PersonData": {
    "Name": "John Doe",
    "Age": 30,
    "Email": "john@example.com"
  }
}
```

### List Configuration

Parse comma-separated values as lists:

```csharp
public interface ISettings
{
    [Option(Alias = "AllowedHosts")]
    List<string> AllowedHosts { get; }

    [Option(Alias = "AllowedPorts")]
    List<int> AllowedPorts { get; }
}
```

**Configuration:**
```xml
<appSettings>
  <add key="AllowedHosts" value="localhost,example.com,test.com"/>
  <add key="AllowedPorts" value="80,443,8080"/>
</appSettings>
```

### Connection Strings

Type-safe access to connection strings:

```csharp
public interface ISettings
{
    [Option(Alias = "MyDatabase", StoreType = SettingStoreType.ConnectionString)]
    string DatabaseConnection { get; }
}
```

**Configuration:**
```xml
<connectionStrings>
  <add name="MyDatabase"
       connectionString="Server=localhost;Database=MyDb;User Id=sa;Password=pass;"
       providerName="System.Data.SqlClient"/>
</connectionStrings>
```

---

## Best Practices

### 1. Always Call MyAppCfg.Configure() at Startup

```csharp
public class Startup
{
    public static void Init()
    {
        MyAppCfg.Configure(
            envVarPrefix: "APPCFG__",
            userSecretsId: "my-app-secrets"
        );
    }
}
```

### 2. Always Provide Default Values

```csharp
// Good - has fallback
[Option(Alias = "MaxRetries", DefaultValue = 3)]
int MaxRetries { get; }

// Bad - throws if not configured
[Option(Alias = "MaxRetries")]
int MaxRetries { get; }
```

### 3. Use User Secrets for Local Development

Never commit secrets to source control!

```bash
# Good: Secrets outside repository
~/.microsoft/usersecrets/my-app/secrets.json

# Bad: Secrets in repository
/MyProject/appsettings.secrets.json  # Never commit this!
```

### 4. Use Environment Variables in Production

```yaml
# Kubernetes example
env:
  - name: APPCFG__ApiKey
    valueFrom:
      secretKeyRef:
        name: app-secrets
        key: api-key
```

### 5. Validate Critical Settings

```csharp
var settings = MyAppCfg.Get<IAppSettings>();

if (string.IsNullOrEmpty(settings.ApiKey))
{
    throw new InvalidOperationException(
        "ApiKey is required! Set APPCFG__ApiKey environment variable.");
}
```

### 6. Use Hierarchical Keys

```csharp
// Good - organized
[Option(Alias = "Database:Host")]
[Option(Alias = "Database:Port")]
[Option(Alias = "Database:Password")]

// Better - with nested interface
public interface IAppSettings
{
    IDatabaseSettings Database { get; }
}
```

### 7. Document Your Configuration

```csharp
public interface IAppSettings
{
    /// <summary>
    /// API key for external service authentication.
    /// Required. Set via APPCFG__ApiKey environment variable.
    /// </summary>
    [Option(Alias = "ApiKey", DefaultValue = "")]
    string ApiKey { get; }

    /// <summary>
    /// Maximum number of retry attempts.
    /// Default: 3. Range: 1-10.
    /// </summary>
    [Option(Alias = "MaxRetries", DefaultValue = 3)]
    int MaxRetries { get; }
}
```

---

## Examples

### Example 1: Console Application

```csharp
using System;
using AppCfg;

class Program
{
    static void Main()
    {
        // Setup
        MyAppCfg.Configure(
            envVarPrefix: "APPCFG__",
            userSecretsId: "myapp"
        );

        // Load
        var settings = MyAppCfg.Get<IAppSettings>();

        // Use
        Console.WriteLine($"App: {settings.AppName} v{settings.Version}");
        Console.WriteLine($"Database: {settings.DatabaseHost}:{settings.DatabasePort}");
    }
}

[DefaultOption(StoreType = SettingStoreType.Custom,
               StoreIdentity = MyAppCfg.DefaultStoreIdentity)]
public interface IAppSettings
{
    [Option(Alias = "AppName", DefaultValue = "MyApp")]
    string AppName { get; }

    [Option(Alias = "Version", DefaultValue = "1.0.0")]
    string Version { get; }

    [Option(Alias = "Database:Host", DefaultValue = "localhost")]
    string DatabaseHost { get; }

    [Option(Alias = "Database:Port", DefaultValue = 5432)]
    int DatabasePort { get; }
}
```

### Example 2: ASP.NET Core

```csharp
using AppCfg;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Configure AppCfg
        MyAppCfg.Configure(
            envVarPrefix: "MYAPP__",
            userSecretsId: "myapp-secrets"
        );

        // Load settings
        var settings = MyAppCfg.Get<IAppSettings>();

        // Register as singleton for DI
        services.AddSingleton(settings);
    }
}

[DefaultOption(StoreType = SettingStoreType.Custom,
               StoreIdentity = MyAppCfg.DefaultStoreIdentity)]
public interface IAppSettings
{
    [Option(Alias = "ConnectionString", DefaultValue = "")]
    string ConnectionString { get; }

    [Option(Alias = "JwtSecret", DefaultValue = "")]
    string JwtSecret { get; }

    [Option(Alias = "CorsOrigins")]
    List<string> CorsOrigins { get; }
}
```

### Example 3: Background Service

```csharp
using AppCfg;
using System.Threading.Tasks;

public class Worker : BackgroundService
{
    private readonly IAppSettings _settings;

    public Worker()
    {
        MyAppCfg.Configure(
            envVarPrefix: "WORKER__",
            userSecretsId: "worker-secrets"
        );

        _settings = MyAppCfg.Get<IAppSettings>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine($"Polling interval: {_settings.PollingInterval}");
            await Task.Delay(_settings.PollingInterval, stoppingToken);
        }
    }
}

[DefaultOption(StoreType = SettingStoreType.Custom,
               StoreIdentity = MyAppCfg.DefaultStoreIdentity)]
public interface IAppSettings
{
    [Option(Alias = "PollingInterval", DefaultValue = "00:01:00")]
    TimeSpan PollingInterval { get; }
}
```

---

## Troubleshooting

### Problem: "Cannot find configuration value"

**Cause:** The key doesn't exist in any configured source.

**Solution:**
1. Verify the key name matches `[Option(Alias = "...")]`
2. Check spelling and casing
3. Add a `DefaultValue` as fallback

```csharp
[Option(Alias = "MyKey", DefaultValue = "fallback-value")]
string MyKey { get; }
```

### Problem: "Type parsing error"

**Cause:** The string value cannot be converted to the target type.

**Solution:**
1. Verify the value format matches the type
2. Check for extra whitespace
3. Use correct format (e.g., `true`/`false` for bool, not `yes`/`no`)

### Problem: "User secrets file not found"

**Cause:** The secrets.json file doesn't exist or is in the wrong location.

**Solution:**
```bash
# Check the path
# Linux/Mac: ~/.microsoft/usersecrets/{userSecretsId}/secrets.json
# Windows: %APPDATA%\Microsoft\UserSecrets\{userSecretsId}\secrets.json

# Create directory
mkdir -p ~/.microsoft/usersecrets/my-app-secrets

# Create file
echo '{"ApiKey":"test"}' > ~/.microsoft/usersecrets/my-app-secrets/secrets.json
```

### Problem: "Environment variables not loading"

**Cause:** Incorrect environment variable name or not using double underscore.

**Solution:**
```bash
# For [Option(Alias = "Database:Host")]

# Correct:
export APPCFG__Database__Host="localhost"

# Wrong:
export APPCFG_Database_Host="localhost"      # Single underscore
export APPCFG:Database:Host="localhost"      # Colon not supported
export APPCFG__DatabaseHost="localhost"      # Missing hierarchy
```

### Problem: "System environment variables not visible"

**Cause:** Set in System Properties but application already running.

**Solution:**
1. Close ALL PowerShell/CMD windows
2. Close Visual Studio / IDE
3. Open FRESH PowerShell window
4. Verify: `$env:APPCFG__ApiKey`
5. Run application

Or set in current session:
```powershell
$env:APPCFG__ApiKey = "test-key"
```

### Problem: "Configuration not checking all sources"

**Cause:** `MyAppCfg.Configure()` was not called at startup.

**Solution:**
```csharp
// Make sure you call Configure() at application startup
MyAppCfg.Configure(
    envVarPrefix: "APPCFG__",
    userSecretsId: "my-app-secrets"
);

// And use the default store identity in your interface
[DefaultOption(StoreType = SettingStoreType.Custom,
               StoreIdentity = MyAppCfg.DefaultStoreIdentity)]
public interface IAppSettings { }
```

### Problem: "Tests are failing with cached values"

**Cause:** UserSecretsStore caches values in memory.

**Solution:**
```csharp
[TearDown]
public void TearDown()
{
    UserSecretsStore.ClearCache();  // Clear cache after each test
}
```

---

## Additional Resources

- **[GETTING_STARTED.md](GETTING_STARTED.md)** - Quick start guide
- **[MIGRATION.md](MIGRATION.md)** - Migrating from older versions
- **[AppCfgDemoUserSecrets](AppCfgDemoUserSecrets/)** - Complete working demo
- **[AppCfgDemoEnvironmentVariables](AppCfgDemoEnvironmentVariables/)** - Environment variables example
- **[AppCfgDemoMssql](AppCfgDemoMssql/)** - Custom store example
- **[AppCfgDemoRedis](AppCfgDemoRedis/)** - Redis store example

---

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## Support & Donate

**If you like this project and would like to support, you can buy me a coffee ☕**

<a href='https://ko-fi.com/I2I13GAGL' target='_blank'><img height='36' style='border:0px;height:36px;' src='https://cdn.ko-fi.com/cdn/kofi4.png?v=2' border='0' alt='Buy Me a Coffee at ko-fi.com' /></a>

**I would appreciate it!**
