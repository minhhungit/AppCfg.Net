# Environment variables / secrets.json review and App.config → secrets.json migration

Date: 2026-09-23

## Goal

1. Verify the environment-variable and user-secrets stores (and `MyAppCfg.Configure()` chaining) behave like .NET Core's configuration providers, and fix what does not.
2. Give .NET Framework projects an easy way to move `appSettings` / `connectionStrings` out of App.config/Web.config into `secrets.json`.

## Review findings (all covered by new tests)

| # | Finding | Fix |
|---|---------|-----|
| 1 | `Configure()` swallowed every user-secrets error, so a malformed secrets.json silently fell through to App.config. | Chain calls `UserSecretsStore.GetValue` directly and lets `AppCfgException` propagate. Missing file still yields null. |
| 2 | Arrays in secrets.json threw. | Flattened to `Key:0`, `Key:1` like the .NET Core JSON provider. |
| 3 | JSON `null` became `""` and broke typed parsing. | Null is skipped so `DefaultValue` applies. |
| 4 | In `Configure()` mode a `SqlConnectionStringBuilder` key present in both `appSettings` and `connectionStrings` picked `appSettings`. | Chain now receives the setting type and checks `connectionStrings` first for that type. |
| 5 | `Configure()` could not be undone; one test passed by accident. | `MyAppCfg.ResetConfiguration()`. |
| 6 | Env-var logic duplicated in `ChainedStore` and `EnvironmentVariableStore`; chain lazily mutated the (non-thread-safe) store registry on first read. | Shared `internal GetValue` on both stores; registry is a `ConcurrentDictionary`; chain no longer touches the registry. |

## Migration design

Two entry points, same semantics:

- **Library:** `AppCfg.SettingStore.UserSecretsMigrator`
  - `MigrateFromConfigFile(path, userSecretsId, options)` parses the XML (honours `file=`, `configSource=`, add/remove/clear).
  - `MigrateFromCurrentConfig(userSecretsId, options)` uses `ConfigurationManager`; skips connection strings inherited from machine.config.
  - `MigrateFromJsonFile(path, userSecretsId, options)` handles .NET Core `appsettings*.json` (reuses the secrets flattening rules).
  - `Migrate(IDictionary, userSecretsId, options)` is the core; `ReadConfigFile(path)` previews.
  - `MigrationOptions`: `IncludeConnectionStrings` (true), `OverwriteExisting` (false), `KeyFilter` (null = all).
  - `MigrationResult`: `SecretsFilePath`, `MigratedKeys`, `SkippedKeys`.
- **CLI:** `AppCfg.Migrate` project, packed as the dotnet global tool `AppCfg.Net.Migrate` (command `appcfg-migrate`, net8.0 with LatestMajor roll-forward). Thin wrapper over `MigrateFromConfigFile`; wildcard `--include/--exclude`, `--no-connection-strings`, `--overwrite`, `--dry-run` (backed by `MigrationOptions.DryRun`). Exit codes: 0 ok, 1 usage, 2 migration error. No unit tests of its own (the net462 test project cannot reference it); verified end to end.
- **Script:** `Scripts/Migrate-AppConfigToUserSecrets.ps1` for projects that cannot add code; supports `-Include/-Exclude` wildcards, `-SkipConnectionStrings`, `-Overwrite`, `-WhatIf`. Runs on Windows PowerShell 5.1 and PowerShell 7.

Rules shared by both:

- Output is flat `"Section:Key": "value"`; an existing nested secrets.json is flattened on merge, values preserved.
- Connection strings are written under their name (what `[Option(Alias = name)] SqlConnectionStringBuilder` looks up). An `appSettings` key with the same name wins.
- Existing keys are kept and reported unless overwrite is requested.
- Nothing is removed from the config file; that is a manual, documented step.

## Out of scope

- Rewriting App.config to strip migrated keys (destructive; left manual).
- Encrypting secrets.json (matches .NET Core, which stores it in plain text under the user profile).
