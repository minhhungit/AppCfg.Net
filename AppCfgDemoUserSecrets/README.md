# AppCfg.Net User Secrets Demo

This demo shows how to use the User Secrets store feature in AppCfg.Net, which is similar to .NET Core's Secret Manager. This allows you to store sensitive configuration data outside of your project tree, keeping secrets out of source control.

## Setup Instructions

### 1. Create the Secrets Directory

The secrets file location depends on your operating system:

**Windows:**
```
%APPDATA%\Microsoft\UserSecrets\appcfg-demo-secrets\secrets.json
```
Example: `C:\Users\YourUsername\AppData\Roaming\Microsoft\UserSecrets\appcfg-demo-secrets\secrets.json`

**Linux/macOS:**
```
~/.microsoft/usersecrets/appcfg-demo-secrets/secrets.json
```
Example: `/home/yourusername/.microsoft/usersecrets/appcfg-demo-secrets/secrets.json`

### 2. Create the secrets.json File

Create the directory and file if they don't exist, then add the following content:

```json
{
  "ApiKey": "sk_test_1234567890abcdef",
  "Database:Password": "MySecurePassword123!",
  "Database:Port": "5432",
  "ClientId": "12345678-1234-1234-1234-123456789abc",
  "Feature:MaxRetries": "5",
  "Feature:Enabled": "true"
}
```

**PowerShell (Windows):**
```powershell
$secretsPath = "$env:APPDATA\Microsoft\UserSecrets\appcfg-demo-secrets"
New-Item -ItemType Directory -Force -Path $secretsPath
Set-Content -Path "$secretsPath\secrets.json" -Value @"
{
  "ApiKey": "sk_test_1234567890abcdef",
  "Database:Password": "MySecurePassword123!",
  "Database:Port": "5432",
  "ClientId": "12345678-1234-1234-1234-123456789abc",
  "Feature:MaxRetries": "5",
  "Feature:Enabled": "true"
}
"@
```

**Bash (Linux/macOS):**
```bash
mkdir -p ~/.microsoft/usersecrets/appcfg-demo-secrets
cat > ~/.microsoft/usersecrets/appcfg-demo-secrets/secrets.json << 'EOF'
{
  "ApiKey": "sk_test_1234567890abcdef",
  "Database:Password": "MySecurePassword123!",
  "Database:Port": "5432",
  "ClientId": "12345678-1234-1234-1234-123456789abc",
  "Feature:MaxRetries": "5",
  "Feature:Enabled": "true"
}
EOF
```

### 3. Build and Run

```bash
# Build the demo
msbuild AppCfgDemoUserSecrets.csproj

# Run the demo
AppCfgDemoUserSecrets.exe
```

Or with Visual Studio, just press F5 to build and run.

## How It Works

### Registration

In `MySettings.cs`, the User Secrets store is registered with a secrets ID:

```csharp
UserSecretsStore.Register("appcfg-demo-secrets");
```

### Configuration

Settings are defined in an interface with the `[Option]` attribute specifying the custom store:

```csharp
public interface ISecretSettings
{
    [Option(
        Alias = "ApiKey",
        StoreType = SettingStoreType.Custom,
        StoreIdentity = "UserSecrets:appcfg-demo-secrets",
        DefaultValue = "")]
    string ApiKey { get; }

    [Option(
        Alias = "Database:Password",
        StoreType = SettingStoreType.Custom,
        StoreIdentity = "UserSecrets:appcfg-demo-secrets",
        DefaultValue = "default-password")]
    string DatabasePassword { get; }
}
```

### Hierarchical Keys

User Secrets supports hierarchical configuration using colon notation:
- JSON: `"Database:Password": "value"`
- Property: `Database:Password`

This matches the .NET Core configuration pattern.

## Features Demonstrated

1. **String secrets** - API keys and passwords
2. **Integer secrets** - Port numbers and retry counts
3. **Boolean secrets** - Feature flags
4. **Guid secrets** - Client IDs
5. **Hierarchical keys** - Using colon notation (Database:Password)
6. **Default values** - Fallback when secrets are missing
7. **Type safety** - Automatic parsing to strongly-typed properties

## Security Notes

1. Secrets are stored as **plain text** in the JSON file
2. Protected by file system permissions (user-only access)
3. **Not suitable for production** - Use Azure Key Vault, HashiCorp Vault, etc. for production
4. **Ideal for local development** - Keeps secrets out of source control
5. Each developer can have their own secrets

## Testing Missing Secrets

To test the default value behavior, rename or delete the secrets.json file and run the demo again. The application will use the default values specified in the `[Option]` attributes.

## Benefits

- **Separation of concerns**: Secrets are not in source control
- **Developer-friendly**: Each developer has their own local secrets
- **Familiar pattern**: Matches .NET Core Secret Manager conventions
- **Type-safe**: Works with AppCfg.Net's type parsing system
- **Flexible**: Can mix with other setting stores (AppSettings, database, Redis, etc.)
