# Tutorial: moving secrets out of App.config / appsettings.json into secrets.json

This walkthrough takes an existing application, moves its sensitive settings into the
per-user `secrets.json` file, and switches the app to AppCfg.Net's priority-based loading:

```
Environment variables  >  secrets.json  >  App.config / Web.config  >  [Option(DefaultValue)]
```

Nothing in the app's `[Option(Alias = "...")]` interfaces changes. Only where the values live changes.

**Requirements**

| Component | Runs on |
|-----------|---------|
| `AppCfg.Net` library (`UserSecretsMigrator`, `UserSecretsStore`, `MyAppCfg.Configure`) | .NET Framework 4.6.2+, .NET Core 2.0+, .NET 5+ |
| `Scripts/Migrate-AppConfigToUserSecrets.ps1` | Windows PowerShell 5.1 or PowerShell 7+, no build needed |
| `appcfg-migrate` command-line tool | Needs a .NET 8+ runtime on the machine running it; the *target app* can still be .NET Framework 4.6.2 |

Pick whichever migration path suits the machine you are on. All three produce the same `secrets.json`.

---

## Part 1 - .NET Framework: App.config / Web.config

### Before

`Web.config` (checked into source control):

```xml
<configuration>
  <appSettings>
    <add key="Api:BaseUrl"   value="https://api.example.com" />
    <add key="Api:Key"       value="sk_live_51H..." />          <!-- secret -->
    <add key="Smtp:Host"     value="smtp.example.com" />
    <add key="Smtp:Password" value="hunter2" />                 <!-- secret -->
  </appSettings>
  <connectionStrings>
    <add name="MainDb" connectionString="Server=.;Database=Shop;User Id=sa;Password=p@ss" />  <!-- secret -->
  </connectionStrings>
</configuration>
```

Settings interface (unchanged throughout):

```csharp
using AppCfg;
using System.Data.SqlClient;

public interface IShopSettings
{
    [Option(Alias = "Api:BaseUrl")]   string ApiBaseUrl { get; }
    [Option(Alias = "Api:Key")]       string ApiKey { get; }
    [Option(Alias = "Smtp:Host")]     string SmtpHost { get; }
    [Option(Alias = "Smtp:Password")] string SmtpPassword { get; }
    [Option(Alias = "MainDb")]        SqlConnectionStringBuilder MainDb { get; }
}
```

### Step 1 - pick a User Secrets ID

Any folder-safe name, one per application, for example `shop-web`. The file will be at:

| OS | Path |
|----|------|
| Windows | `%APPDATA%\Microsoft\UserSecrets\shop-web\secrets.json` |
| Linux / macOS | `~/.microsoft/usersecrets/shop-web/secrets.json` |

This is the same convention as .NET Core's `<UserSecretsId>` / `dotnet user-secrets`.

### Step 2 - preview what would move (dry run)

Choose one:

**A. Command-line tool**

```powershell
dotnet tool install -g AppCfg.Net.Migrate      # once
appcfg-migrate .\Web.config --id shop-web -i "*Key" -i "*Password*" -i "MainDb" --dry-run
```

**B. PowerShell script (no .NET 8 needed)**

```powershell
.\Scripts\Migrate-AppConfigToUserSecrets.ps1 -ConfigPath .\Web.config -UserSecretsId shop-web `
    -Include '*Key','*Password*','MainDb' -WhatIf
```

**C. C# (for example a throw-away console app or a unit test)**

```csharp
using AppCfg.SettingStore;

var preview = UserSecretsMigrator.MigrateFromConfigFile(@"C:\src\Shop\Web.config", "shop-web",
    new MigrationOptions
    {
        DryRun = true,
        KeyFilter = key => key.EndsWith("Key") || key.Contains("Password") || key == "MainDb"
    });

Console.WriteLine($"Would write {preview.MigratedKeys.Count} keys to {preview.SecretsFilePath}");
foreach (var key in preview.MigratedKeys) Console.WriteLine("  + " + key);
foreach (var key in preview.SkippedKeys)  Console.WriteLine("  = " + key + " (already in secrets.json)");
```

Expected output for the sample:

```
DRY RUN - nothing was written.
Secrets file : C:\Users\you\AppData\Roaming\Microsoft\UserSecrets\shop-web\secrets.json
Migrated     : 3
  + Api:Key
  + Smtp:Password
  + MainDb
```

Tips for filters:

- `-i` / `--include` are wildcards, case-insensitive, repeatable. `-e` / `--exclude` removes matches.
- Leave the filters out to migrate *every* key. That also works, but the point of secrets.json is secrets; keep non-sensitive defaults in the config file.
- Connection strings are migrated under their `name` so `[Option(Alias = "MainDb")]` keeps working. Use `--no-connection-strings` to leave them.
- Standalone section files also work: `appcfg-migrate .\@Configs\AppSettings.TEST.config --id shop-web` reads a file whose root is `<appSettings>` (as used by `<appSettings file="...">` or `configSource`).

### Step 3 - run it for real

Same command without `--dry-run` / `-WhatIf` / `DryRun = true`:

```powershell
appcfg-migrate .\Web.config --id shop-web -i "*Key" -i "*Password*" -i "MainDb"
```

Result, `%APPDATA%\Microsoft\UserSecrets\shop-web\secrets.json`:

```json
{
  "Api:Key": "sk_live_51H...",
  "Smtp:Password": "hunter2",
  "MainDb": "Server=.;Database=Shop;User Id=sa;Password=p@ss"
}
```

Running again is safe: keys already in `secrets.json` are kept and listed under "Kept existing".
Add `--overwrite` (`-Overwrite`, `OverwriteExisting = true`) to replace them.

### Step 4 - remove the secrets from the config file

Delete the migrated entries. Keep the non-secret ones:

```xml
<configuration>
  <appSettings>
    <add key="Api:BaseUrl" value="https://api.example.com" />
    <add key="Smtp:Host"   value="smtp.example.com" />
  </appSettings>
  <connectionStrings />
</configuration>
```

Commit this. From now on the repository has no secrets.

### Step 5 - switch the app to priority-based loading

Add one line at startup (`Global.asax.cs` `Application_Start`, `Program.Main`, `Startup`, ...):

```csharp
using AppCfg;

MyAppCfg.Configure(envVarPrefix: "SHOP__", userSecretsId: "shop-web");
```

That is the whole code change. Every interface without an explicit `ProfileKey` now reads
environment variables first, then `secrets.json`, then `App.config` / `Web.config`.

### Step 6 - verify

```csharp
var s = MyAppCfg.Get<IShopSettings>();
Console.WriteLine(s.ApiBaseUrl);          // https://api.example.com   (from Web.config)
Console.WriteLine(s.ApiKey);              // sk_live_51H...            (from secrets.json)
Console.WriteLine(s.MainDb.InitialCatalog); // Shop                    (from secrets.json)
```

A quick NUnit/xUnit check you can keep:

```csharp
[Test]
public void Secrets_are_loaded_from_secrets_json_not_config()
{
    MyAppCfg.Configure(envVarPrefix: "SHOP__", userSecretsId: "shop-web");
    var s = MyAppCfg.Get<IShopSettings>();

    Assert.IsNotNull(s.ApiKey, "Api:Key should come from secrets.json");
    Assert.IsNull(System.Configuration.ConfigurationManager.AppSettings["Api:Key"],
        "Api:Key must no longer be in Web.config");
}
```

### Step 7 - other machines and production

Each developer runs Step 2/3 once on their machine (or writes the file by hand). Servers and
containers do not use `secrets.json`; they set environment variables, which sit above it:

```
Setting key      Environment variable
Api:Key      ->  SHOP__Api__Key
Smtp:Password -> SHOP__Smtp__Password
MainDb       ->  SHOP__MainDb
```

(`:` becomes `__`, the prefix is whatever you passed as `envVarPrefix`.)

```powershell
# Windows service / IIS app pool (machine level)
[Environment]::SetEnvironmentVariable("SHOP__Api__Key", "sk_live_...", "Machine")
```

```bash
# Docker
docker run -e SHOP__Api__Key=sk_live_... -e SHOP__MainDb="Server=db;..." shop-web
```

---

## Part 2 - .NET Core / .NET 5+: appsettings.json

AppCfg.Net does not read `appsettings.json` itself. Its sources are environment variables,
`secrets.json`, `App.config` (which works on .NET Core through
`System.Configuration.ConfigurationManager`, already referenced by AppCfg.Net) and custom stores.
So the migration for a .NET Core app is:

- secrets in `appsettings*.json` -> `secrets.json` (this part)
- non-secret defaults -> keep them in an `App.config`, or put them on the interface with
  `DefaultValue`, or keep using `Microsoft.Extensions.Configuration` for them and let AppCfg own only
  what it needs.

### Before

`appsettings.Development.json`:

```json
{
  "ConnectionStrings": { "MainDb": "Server=.;Database=Shop;User Id=sa;Password=p@ss" },
  "Api": { "BaseUrl": "https://api.example.com", "Key": "sk_live_51H..." },
  "Smtp": { "Host": "smtp.example.com", "Password": "hunter2" },
  "Logging": { "LogLevel": { "Default": "Information" } }
}
```

Nested JSON maps to colon keys, the same way `Microsoft.Extensions.Configuration` does it:
`Api:Key`, `ConnectionStrings:MainDb`, `Logging:LogLevel:Default`. Arrays become `Hosts:0`, `Hosts:1`.

Settings interface:

```csharp
public interface IShopSettings
{
    [Option(Alias = "Api:BaseUrl", DefaultValue = "https://api.example.com")] string ApiBaseUrl { get; }
    [Option(Alias = "Api:Key")]                     string ApiKey { get; }
    [Option(Alias = "Smtp:Host", DefaultValue = "smtp.example.com")] string SmtpHost { get; }
    [Option(Alias = "Smtp:Password")]               string SmtpPassword { get; }
    [Option(Alias = "ConnectionStrings:MainDb")]    SqlConnectionStringBuilder MainDb { get; }
}
```

### Step 1 - User Secrets ID

If the project already has `<UserSecretsId>` in its `.csproj` (Visual Studio's *Manage User Secrets*
adds one), reuse it so AppCfg and `dotnet user-secrets` share the same file:

```xml
<PropertyGroup>
  <UserSecretsId>shop-web</UserSecretsId>
</PropertyGroup>
```

### Step 2 - preview

```powershell
appcfg-migrate .\appsettings.Development.json --id shop-web `
    -i "ConnectionStrings:*" -i "*Key" -i "*Password*" --dry-run
```

or in C#:

```csharp
var preview = UserSecretsMigrator.MigrateFromJsonFile(@"C:\src\Shop\appsettings.Development.json", "shop-web",
    new MigrationOptions
    {
        DryRun = true,
        KeyFilter = key => key.StartsWith("ConnectionStrings:") || key.EndsWith("Key") || key.Contains("Password")
    });
```

Output:

```
DRY RUN - nothing was written.
Migrated     : 3
  + ConnectionStrings:MainDb
  + Api:Key
  + Smtp:Password
```

### Step 3 - migrate

Drop `--dry-run`. Resulting `secrets.json`:

```json
{
  "ConnectionStrings:MainDb": "Server=.;Database=Shop;User Id=sa;Password=p@ss",
  "Api:Key": "sk_live_51H...",
  "Smtp:Password": "hunter2"
}
```

This is exactly the layout `dotnet user-secrets set "Api:Key" "..."` produces, so the two tools can
be mixed. If a `secrets.json` already existed with nested objects, it is flattened on merge and its
values are preserved.

### Step 4 - clean appsettings.json

```json
{
  "Api":  { "BaseUrl": "https://api.example.com" },
  "Smtp": { "Host": "smtp.example.com" },
  "Logging": { "LogLevel": { "Default": "Information" } }
}
```

Delete the now-empty `ConnectionStrings` section. Commit.

### Step 5 - wire up AppCfg

```csharp
// Program.cs
using AppCfg;

MyAppCfg.Configure(envVarPrefix: "SHOP__", userSecretsId: "shop-web");

var builder = WebApplication.CreateBuilder(args);
// ... the host keeps reading appsettings.json for Logging, Kestrel, etc.
builder.Services.AddSingleton(MyAppCfg.Get<IShopSettings>());
```

Non-secret values that AppCfg should also see: either give them `DefaultValue` as above, or add an
`App.config` to the project (it is copied to `Shop.dll.config` and read by `ConfigurationManager`):

```xml
<configuration>
  <appSettings>
    <add key="Api:BaseUrl" value="https://api.example.com" />
    <add key="Smtp:Host"   value="smtp.example.com" />
  </appSettings>
</configuration>
```

### Step 6 - production

Same as Part 1, Step 7: environment variables with `__`:

```bash
SHOP__ConnectionStrings__MainDb="Server=db;..."
SHOP__Api__Key=sk_live_...
```

---

## Reference

### Key naming cheat-sheet

| In `[Option(Alias = ...)]` | secrets.json (flat) | secrets.json (nested) | Environment variable (`SHOP__` prefix) |
|---|---|---|---|
| `Api:Key` | `"Api:Key": "..."` | `{ "Api": { "Key": "..." } }` | `SHOP__Api__Key` |
| `MainDb` | `"MainDb": "..."` | | `SHOP__MainDb` |
| `Hosts:0` | `"Hosts:0": "..."` | `{ "Hosts": ["..."] }` | `SHOP__Hosts__0` |

Lookups are case-insensitive in all three sources.

### `MigrationOptions`

| Property | Default | Meaning |
|---|---|---|
| `KeyFilter` | `null` (all keys) | `Func<string,bool>`; only keys returning true are migrated |
| `IncludeConnectionStrings` | `true` | Migrate `<connectionStrings>` by name (config files only) |
| `OverwriteExisting` | `false` | Replace keys already present in secrets.json; otherwise they are reported in `SkippedKeys` |
| `DryRun` | `false` | Compute the result but write nothing |

### `UserSecretsMigrator` entry points

| Method | Source |
|---|---|
| `MigrateFromConfigFile(path, id, options)` | App.config, Web.config, `*.exe.config`, or a standalone `<appSettings>` / `<connectionStrings>` file. Honours `file=`, `configSource=`, `<remove>`, `<clear>`. |
| `MigrateFromJsonFile(path, id, options)` | appsettings.json and friends; nested objects and arrays flattened |
| `MigrateFromCurrentConfig(id, options)` | Whatever `ConfigurationManager` sees in the running process (skips machine.config's `LocalSqlServer`) |
| `Migrate(IDictionary<string,string>, id, options)` | Any source you assembled yourself |
| `ReadConfigFile(path)` | Parse a config file into a dictionary without writing |
| `UserSecretsStore.GetSecretsFilePath(id)` | Where the file is |

### Troubleshooting

**`Migrated: 0`** - the file has no `<appSettings>` under `<configuration>`. Standalone files whose root
*is* `<appSettings>` are supported; anything else (for example a `<system.web>` only file) has nothing to migrate.

**Value still comes from App.config after migrating** - `Configure()` was not called, or was called with a
different `userSecretsId`, or `secrets.json` is cached from before the migration in a long-running process.
`UserSecretsStore.ClearCache()` forces a re-read; the migrator already does this for the ID it wrote.

**`AppCfgException: Invalid JSON in secrets file`** - `secrets.json` is malformed. This is reported on purpose
rather than silently falling back to App.config.

**Connection string key exists in both `appSettings` and `connectionStrings`** - for a `SqlConnectionStringBuilder`
property the `connectionStrings` entry wins; for a `string` property `appSettings` wins. In secrets.json there is
one namespace, and the migrator lets the `appSettings` value win and reports the other as skipped.

**Team members** - secrets.json is per user and never committed. Share the *list of keys* (a `secrets.example.json`
in the repo is a common pattern), not the values.
