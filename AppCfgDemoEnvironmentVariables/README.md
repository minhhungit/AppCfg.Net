# AppCfg.Net Environment Variables Demo

This demo shows how to use the Environment Variables store feature in AppCfg.Net, which reads configuration from environment variables. This is perfect for containerized applications, CI/CD pipelines, and cloud-native deployments.

## Features Demonstrated

1. **Environment Variables Store** - Read settings from environment variables
2. **DefaultOption Attribute** - Set default StoreType and StoreIdentity at interface level to avoid repetition
3. **Hierarchical Keys** - Use colon notation in property names, converted to double underscore in env vars
4. **Mixed Sources** - Combine environment variables with traditional AppSettings

## Setup Instructions

### 1. Set Environment Variables

The environment variables use the format: `APPCFG__{Key}__{SubKey}`

Colons (`:`) in property aliases become double underscores (`__`) in environment variable names.

**PowerShell (Windows):**
```powershell
$env:APPCFG__ApiKey="sk_test_1234567890"
$env:APPCFG__Database__Host="localhost"
$env:APPCFG__Database__Port="5432"
$env:APPCFG__Database__Password="MySecurePassword123"
$env:APPCFG__Feature__Enabled="true"
$env:APPCFG__Feature__MaxRetries="5"
$env:APPCFG__ClientId="12345678-1234-1234-1234-123456789abc"
$env:APPCFG__SecretKey="my-secret-key"
```

**Bash (Linux/macOS):**
```bash
export APPCFG__ApiKey="sk_test_1234567890"
export APPCFG__Database__Host="localhost"
export APPCFG__Database__Port="5432"
export APPCFG__Database__Password="MySecurePassword123"
export APPCFG__Feature__Enabled="true"
export APPCFG__Feature__MaxRetries="5"
export APPCFG__ClientId="12345678-1234-1234-1234-123456789abc"
export APPCFG__SecretKey="my-secret-key"
```

**CMD (Windows):**
```cmd
set APPCFG__ApiKey=sk_test_1234567890
set APPCFG__Database__Host=localhost
set APPCFG__Database__Port=5432
set APPCFG__Database__Password=MySecurePassword123
set APPCFG__Feature__Enabled=true
set APPCFG__Feature__MaxRetries=5
set APPCFG__ClientId=12345678-1234-1234-1234-123456789abc
set APPCFG__SecretKey=my-secret-key
```

### 2. Build and Run

```bash
# Build the demo
msbuild AppCfgDemoEnvironmentVariables.csproj

# Run the demo
AppCfgDemoEnvironmentVariables.exe
```

Or with Visual Studio, just press F5 to build and run.

## How It Works

### Registration

In `MySettings.cs`, the Environment Variables store is registered with default prefix:

```csharp
EnvironmentVariableStore.Register();  // Uses "APPCFG__" prefix
```

You can also use a custom prefix:

```csharp
EnvironmentVariableStore.Register("MYAPP__");
```

### DefaultOption Attribute (NEW!)

Instead of repeating `StoreType` and `StoreIdentity` on every property, use the `[DefaultOption]` attribute at the interface level:

**BEFORE (repetitive):**
```csharp
public interface ISettings
{
    [Option(Alias = "ApiKey",
            StoreType = SettingStoreType.Custom,
            StoreIdentity = "EnvironmentVariables:APPCFG__")]
    string ApiKey { get; }

    [Option(Alias = "Database:Host",
            StoreType = SettingStoreType.Custom,
            StoreIdentity = "EnvironmentVariables:APPCFG__")]
    string DatabaseHost { get; }
}
```

**AFTER (clean):**
```csharp
[DefaultOption(StoreType = SettingStoreType.Custom,
               StoreIdentity = "EnvironmentVariables:APPCFG__")]
public interface ISettings
{
    [Option(Alias = "ApiKey")]
    string ApiKey { get; }

    [Option(Alias = "Database:Host")]
    string DatabaseHost { get; }
}
```

### Hierarchical Keys

Environment variables support hierarchical configuration using double underscores:

| Property Alias      | Environment Variable             |
|---------------------|----------------------------------|
| `ApiKey`            | `APPCFG__ApiKey`                |
| `Database:Host`     | `APPCFG__Database__Host`        |
| `Database:Port`     | `APPCFG__Database__Port`        |
| `Feature:MaxRetries`| `APPCFG__Feature__MaxRetries`   |

This matches the ASP.NET Core configuration convention.

### Mixed Sources

You can override the default store for specific properties:

```csharp
[DefaultOption(StoreType = SettingStoreType.Custom,
               StoreIdentity = "EnvironmentVariables:APPCFG__")]
public interface IMixedSettings
{
    // Uses environment variable (from DefaultOption)
    [Option(Alias = "SecretKey")]
    string SecretKey { get; }

    // Overrides to use AppSettings instead
    // IMPORTANT: StoreIdentity must be set (even to empty string) to override
    [Option(Alias = "PublicSetting",
            StoreType = SettingStoreType.AppSetting,
            StoreIdentity = "")]
    string PublicSetting { get; }
}
```

**Override Rule:** To override the DefaultOption settings, you must explicitly set `StoreIdentity` on the property. Set it to empty string (`""`) when overriding to AppSettings.

## Type Support

Environment Variables store supports all the same types as other stores:
- String
- Int, Long, Decimal, Double
- Boolean
- Guid
- DateTime, TimeSpan
- Enums
- Lists

Values are automatically parsed from strings to the target type.

## Use Cases

1. **Docker/Kubernetes** - Perfect for containerized applications
2. **CI/CD Pipelines** - Easy to inject secrets during deployment
3. **Cloud Platforms** - Standard pattern on Azure, AWS, GCP
4. **12-Factor Apps** - Follows the config best practices
5. **Development** - Quick configuration without file changes

## Benefits

- **No files needed:** Configuration through environment only
- **Container-friendly:** Standard pattern for Docker/Kubernetes
- **CI/CD integration:** Easy secret injection in pipelines
- **Cloud-native:** Works everywhere
- **Type-safe:** Automatic parsing and validation
- **Clean code:** DefaultOption reduces boilerplate

## Security Notes

1. Environment variables are visible to the process and can be listed
2. Use proper access controls on the host system
3. For production, consider:
   - Azure Key Vault
   - AWS Secrets Manager
   - HashiCorp Vault
   - Kubernetes Secrets
4. Environment variables are good for non-production or well-secured environments
