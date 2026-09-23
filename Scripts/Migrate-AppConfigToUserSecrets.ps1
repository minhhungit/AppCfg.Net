<#
.SYNOPSIS
    Migrates appSettings and connectionStrings from a .NET Framework App.config / Web.config
    into the user-secrets file (secrets.json) that AppCfg.Net and the .NET Core Secret Manager read.

.DESCRIPTION
    Reads the given config file (honouring <appSettings file="...">, configSource="..." and
    add/remove/clear), merges the entries into
        Windows : %APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json
        Linux/Mac: ~/.microsoft/usersecrets/<UserSecretsId>/secrets.json
    and prints what was migrated. Existing secrets are kept unless -Overwrite is given.
    Keys are written flat ("Database:Host": "...") which is exactly what
    MyAppCfg.Configure(userSecretsId: "<UserSecretsId>") expects.

    Works in Windows PowerShell 5.1 and PowerShell 7+. Nothing to build - no dependency on AppCfg.

.PARAMETER ConfigPath
    Path to App.config, Web.config or MyApp.exe.config.

.PARAMETER UserSecretsId
    The user secrets ID you pass to MyAppCfg.Configure(userSecretsId: ...).

.PARAMETER Include
    Wildcard patterns; only keys matching at least one are migrated. Default: everything.
    Example: -Include '*Password*','*Secret*','*Key','*ConnectionString*'

.PARAMETER Exclude
    Wildcard patterns; keys matching any of them are skipped.

.PARAMETER SkipConnectionStrings
    Do not migrate <connectionStrings>. By default they are migrated under their name.

.PARAMETER Overwrite
    Replace keys that already exist in secrets.json. Default: keep the existing value.

.EXAMPLE
    .\Migrate-AppConfigToUserSecrets.ps1 -ConfigPath .\Web.config -UserSecretsId my-app-secrets

.EXAMPLE
    .\Migrate-AppConfigToUserSecrets.ps1 -ConfigPath .\App.config -UserSecretsId my-app-secrets `
        -Include '*Password*','*ApiKey*' -WhatIf
#>
[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [Parameter(Mandatory = $true)]
    [string]$ConfigPath,

    [Parameter(Mandatory = $true)]
    [string]$UserSecretsId,

    [string[]]$Include = @('*'),

    [string[]]$Exclude = @(),

    [switch]$SkipConnectionStrings,

    [switch]$Overwrite
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = 'Stop'

function Get-SecretsFilePath([string]$Id) {
    if ($env:APPDATA) {
        return Join-Path $env:APPDATA "Microsoft\UserSecrets\$Id\secrets.json"
    }
    if ($env:HOME) {
        return Join-Path $env:HOME ".microsoft/usersecrets/$Id/secrets.json"
    }
    throw "Neither APPDATA nor HOME is set; cannot locate the user secrets folder."
}

function Read-XmlFile([string]$Path) {
    $xml = New-Object System.Xml.XmlDocument
    $xml.Load((Resolve-Path $Path).Path)
    return $xml
}

# Applies <add/remove/clear> children of $Section to $Target (an ordered, case-insensitive table).
function Apply-Section([System.Xml.XmlElement]$Section, [System.Collections.Specialized.OrderedDictionary]$Target,
                       [string]$KeyAttr, [string]$ValueAttr) {
    if ($null -eq $Section) { return }
    foreach ($child in $Section.ChildNodes) {
        if ($child.NodeType -ne 'Element') { continue }
        switch ($child.LocalName) {
            'add' {
                $k = $child.GetAttribute($KeyAttr)
                if ($k) { $Target[$k] = [string]$child.GetAttribute($ValueAttr) }
            }
            'remove' {
                $k = $child.GetAttribute($KeyAttr)
                if ($k -and $Target.Contains($k)) { $Target.Remove($k) }
            }
            'clear' { $Target.Clear() }
        }
    }
}

# Returns the section element itself, or the root of its configSource file.
function Resolve-ConfigSource([System.Xml.XmlElement]$Section, [string]$BaseDir) {
    $src = $Section.GetAttribute('configSource')
    if (-not $src) { return $Section }
    $path = Join-Path $BaseDir $src
    if (-not (Test-Path $path)) { throw "configSource file for <$($Section.LocalName)> not found: $path" }
    return (Read-XmlFile $path).DocumentElement
}

function New-OrderedTable {
    # Ordered + case-insensitive, matching how secrets are looked up
    return New-Object System.Collections.Specialized.OrderedDictionary([System.StringComparer]::OrdinalIgnoreCase)
}

# Flattens a parsed JSON object into "A:B:0" keys, like the .NET Core JSON provider.
function Flatten-Json($Node, [string]$Prefix, [System.Collections.Specialized.OrderedDictionary]$Target) {
    if ($null -eq $Node) { return }
    if ($Node -is [System.Management.Automation.PSCustomObject]) {
        foreach ($p in $Node.PSObject.Properties) {
            $key = if ($Prefix) { "$Prefix`:$($p.Name)" } else { $p.Name }
            Flatten-Json $p.Value $key $Target
        }
    }
    elseif ($Node -is [System.Array]) {
        for ($i = 0; $i -lt $Node.Length; $i++) {
            Flatten-Json $Node[$i] "$Prefix`:$i" $Target
        }
    }
    else {
        $Target[$Prefix] = [string]$Node
    }
}

function Test-KeyMatches([string]$Key) {
    $included = $false
    foreach ($p in $Include) { if ($Key -like $p) { $included = $true; break } }
    if (-not $included) { return $false }
    foreach ($p in $Exclude) { if ($Key -like $p) { return $false } }
    return $true
}

# ---- 1. Read the config file ---------------------------------------------------------------

if (-not (Test-Path $ConfigPath)) { throw "Config file not found: $ConfigPath" }
$configFull = (Resolve-Path $ConfigPath).Path
$baseDir = Split-Path $configFull -Parent
$doc = Read-XmlFile $configFull
$root = $doc.DocumentElement

$settings = New-OrderedTable

$appSettings = $root.SelectSingleNode('appSettings')
if ($appSettings) {
    Apply-Section (Resolve-ConfigSource $appSettings $baseDir) $settings 'key' 'value'
    $extFile = $appSettings.GetAttribute('file')
    if ($extFile) {
        $extPath = Join-Path $baseDir $extFile
        if (Test-Path $extPath) {   # .NET silently ignores a missing file= target
            Apply-Section (Read-XmlFile $extPath).DocumentElement $settings 'key' 'value'
        }
    }
}

if (-not $SkipConnectionStrings) {
    $connStrings = $root.SelectSingleNode('connectionStrings')
    if ($connStrings) {
        $conns = New-OrderedTable
        Apply-Section (Resolve-ConfigSource $connStrings $baseDir) $conns 'name' 'connectionString'
        foreach ($name in @($conns.Keys)) {
            if (-not $settings.Contains($name)) { $settings[$name] = $conns[$name] }
        }
    }
}

# ---- 2. Load existing secrets.json ---------------------------------------------------------

$secretsPath = Get-SecretsFilePath $UserSecretsId
$existing = New-OrderedTable
if (Test-Path $secretsPath) {
    $raw = Get-Content $secretsPath -Raw
    if ($raw -and $raw.Trim()) {
        Flatten-Json ($raw | ConvertFrom-Json) '' $existing
    }
}

# ---- 3. Merge --------------------------------------------------------------------------------

$output = New-OrderedTable
foreach ($k in @($existing.Keys)) { $output[$k] = $existing[$k] }

$migrated = New-Object System.Collections.Generic.List[string]
$skipped  = New-Object System.Collections.Generic.List[string]
$filtered = New-Object System.Collections.Generic.List[string]

foreach ($k in @($settings.Keys)) {
    if (-not (Test-KeyMatches $k)) { $filtered.Add($k); continue }
    if ($output.Contains($k) -and -not $Overwrite) { $skipped.Add($k); continue }
    $output[$k] = $settings[$k]
    $migrated.Add($k)
}

# ---- 4. Write ------------------------------------------------------------------------------

$json = $output | ConvertTo-Json -Depth 2
if ($output.Count -eq 0) { $json = '{}' }

if ($PSCmdlet.ShouldProcess($secretsPath, "Write $($migrated.Count) secret(s)")) {
    $dir = Split-Path $secretsPath -Parent
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    [System.IO.File]::WriteAllText($secretsPath, $json, (New-Object System.Text.UTF8Encoding($false)))
}

# ---- 5. Report -----------------------------------------------------------------------------

Write-Host ""
Write-Host "Secrets file : $secretsPath"
Write-Host "Migrated     : $($migrated.Count)" -ForegroundColor Green
foreach ($k in $migrated) { Write-Host "  + $k" -ForegroundColor Green }
if ($skipped.Count) {
    Write-Host "Kept existing: $($skipped.Count)  (use -Overwrite to replace)" -ForegroundColor Yellow
    foreach ($k in $skipped) { Write-Host "  = $k" -ForegroundColor Yellow }
}
if ($filtered.Count) {
    Write-Host "Not selected : $($filtered.Count)  (Include/Exclude filters)" -ForegroundColor DarkGray
}
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Remove the migrated keys from $([System.IO.Path]::GetFileName($configFull)) so they leave source control."
Write-Host "  2. At startup call:  MyAppCfg.Configure(envVarPrefix: `"MYAPP__`", userSecretsId: `"$UserSecretsId`");"
Write-Host "  3. Your [Option(Alias = ...)] keys stay the same - secrets.json now overrides App.config."
