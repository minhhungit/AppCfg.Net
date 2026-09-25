# AppCfg.Net Architecture

This document provides visual diagrams of the AppCfg.Net configuration library architecture using Mermaid syntax.

## Table of Contents

1. [System Architecture Overview](#1-system-architecture-overview)
2. [Configuration Data Flow](#2-configuration-data-flow)
3. [Type Parser Hierarchy](#3-type-parser-hierarchy)
4. [Setting Store System](#4-setting-store-system)
5. [Feature Overview](#5-feature-overview)
6. [Attribute System](#6-attribute-system)
7. [Multi-Tenancy Flow](#7-multi-tenancy-flow)
8. [Complete Class Diagram](#8-complete-class-diagram)

---

## 1. System Architecture Overview

```mermaid
flowchart TB
    subgraph UserCode["User Code"]
        Call["MyAppCfg.Get&lt;ISettings&gt;()"]
    end

    subgraph Core["AppCfg Core"]
        MyAppCfg["MyAppCfg<br/>━━━━━━━━━━<br/>Configure()<br/>Get&lt;T&gt;()<br/>Get&lt;T&gt;(tenantKey)"]
        Mixer["AppCfgTypeMixer<br/>━━━━━━━━━━<br/>Dynamic Type Generation<br/>via Reflection.Emit"]
        RawCreator["RawValueCreator<br/>━━━━━━━━━━<br/>Coordinate Store Lookup"]
    end

    subgraph Registries["Registries"]
        TypeParsers["TypeParsers<br/>━━━━━━━━━━<br/>Register&lt;T&gt;()<br/>Get(Type)"]
        SettingStores["SettingStores<br/>━━━━━━━━━━<br/>RegisterStore()<br/>Get(profileKey)"]
    end

    subgraph Attributes["Attributes"]
        Option["[Option]<br/>Alias, Default,<br/>ProfileKey"]
        DefaultOption["[DefaultOption]<br/>Interface-level"]
        Computed["[Computed]<br/>Static method ref"]
    end

    subgraph Stores["Setting Stores"]
        Chain["ChainedStore"]
        EnvStore["EnvironmentVariableStore"]
        SecretStore["UserSecretsStore"]
        AppConfig["App.config"]
        CustomStore["Custom Stores"]
    end

    subgraph Parsers["Type Parsers"]
        Scalar["Scalar Parsers<br/>int, string, bool..."]
        Collection["Collection Parsers<br/>List&lt;T&gt;, IReadOnlyList"]
        Special["Special Parsers<br/>JSON, Enum, ConnString"]
    end

    Result["Typed Settings Instance"]

    Call --> MyAppCfg
    MyAppCfg --> Mixer
    MyAppCfg --> RawCreator
    RawCreator --> SettingStores
    SettingStores --> Stores
    Chain --> EnvStore
    Chain --> SecretStore
    Chain --> AppConfig
    MyAppCfg --> TypeParsers
    TypeParsers --> Parsers
    Mixer --> Attributes
    Parsers --> Result
    Result --> Call
```

---

## 2. Configuration Data Flow

```mermaid
sequenceDiagram
    participant User as User Code
    participant Cfg as MyAppCfg
    participant Mixer as AppCfgTypeMixer
    participant Store as SettingStore
    participant Parser as TypeParser
    participant Instance as Settings Instance

    User->>Cfg: Get<IAppSettings>(tenantKey?)

    Cfg->>Mixer: ExtendWith<IAppSettings>()
    Mixer-->>Cfg: Concrete type via IL emit
    Cfg->>Instance: Create instance

    loop For each property
        Cfg->>Cfg: Get [Option] attributes
        Cfg->>Cfg: Resolve ProfileKey
        Cfg->>Store: GetRawValue(key, tenant)

        alt ChainedStore configured
            Store->>Store: 1. Check Environment
            Store->>Store: 2. Check UserSecrets
            Store->>Store: 3. Check App.config
        end

        Store-->>Cfg: Raw string value
        Cfg->>Parser: Get parser for Type
        Cfg->>Parser: Parse(rawValue, options)
        Parser-->>Cfg: Typed value
        Cfg->>Instance: Set property value
    end

    opt Has [Computed] properties
        Cfg->>Cfg: Find static method
        Cfg->>Cfg: Invoke with instance
        Cfg->>Instance: Set computed value
    end

    Instance-->>User: Return IAppSettings
```

---

## 3. Type Parser Hierarchy

```mermaid
classDiagram
    class ITypeParser~T~ {
        <<interface>>
        +Parse(rawValue, options) T
    }

    class ITypeParserRawBuilder~T~ {
        <<interface>>
        +GetRawValue(settingKey) string
    }

    class ITypeParserOptions {
        <<interface>>
        +Alias string
        +DefaultValue object
        +RawValue string
        +InputFormat string
        +Separator string
        +ProfileKey string
    }

    ITypeParserRawBuilder --|> ITypeParser : extends

    class BooleanParser {
        +Parse() bool
    }
    class IntParser {
        +Parse() int
    }
    class LongParser {
        +Parse() long
    }
    class DecimalParser {
        +Parse() decimal
    }
    class DoubleParser {
        +Parse() double
    }
    class StringParser {
        +Parse() string
    }
    class DateTimeParser {
        +Parse() DateTime
    }
    class TimeSpanParser {
        +Parse() TimeSpan
    }
    class GuidParser {
        +Parse() Guid
    }

    class ListIntParser {
        +Parse() List~int~
    }
    class ListStringParser {
        +Parse() List~string~
    }
    class ListEnumParser~T~ {
        +Parse() List~T~
    }

    class JsonParser~T~ {
        +Parse() T
    }
    class EnumParser~T~ {
        +Parse() T
    }
    class ConnectionStringParser {
        +Parse() SqlConnectionStringBuilder
        +GetRawValue() string
    }

    ITypeParser <|.. BooleanParser
    ITypeParser <|.. IntParser
    ITypeParser <|.. LongParser
    ITypeParser <|.. DecimalParser
    ITypeParser <|.. DoubleParser
    ITypeParser <|.. StringParser
    ITypeParser <|.. DateTimeParser
    ITypeParser <|.. TimeSpanParser
    ITypeParser <|.. GuidParser
    ITypeParser <|.. ListIntParser
    ITypeParser <|.. ListStringParser
    ITypeParser <|.. ListEnumParser
    ITypeParser <|.. JsonParser
    ITypeParser <|.. EnumParser
    ITypeParserRawBuilder <|.. ConnectionStringParser
```

---

## 4. Setting Store System

```mermaid
flowchart LR
    subgraph Sources["Configuration Sources"]
        ENV["Environment Variables<br/>PREFIX__Section__Key"]
        SECRETS["User Secrets<br/>secrets.json"]
        CONFIG["App.config<br/>AppSettings"]
        DB["Database<br/>Custom Store"]
        REDIS["Redis<br/>Custom Store"]
    end

    subgraph ChainedStore["ChainedStore (Priority)"]
        direction TB
        P1["Priority 1: Environment"]
        P2["Priority 2: UserSecrets"]
        P3["Priority 3: App.config"]
        P1 --> P2
        P2 --> P3
    end

    subgraph Registry["SettingStores Registry"]
        REG["ProfileKey -> Store Function<br/>━━━━━━━━━━━━━━━━━<br/>null -> ChainedStore<br/>'env' -> EnvStore<br/>'db' -> DatabaseStore<br/>'redis' -> RedisStore"]
    end

    ENV --> P1
    SECRETS --> P2
    CONFIG --> P3
    DB --> REG
    REDIS --> REG
    ChainedStore --> REG

    REG --> |"First non-null<br/>value wins"| OUTPUT["Raw Value"]
```

---

## 5. Feature Overview

```mermaid
mindmap
    root((AppCfg.Net))
        Core Features
            Type-Safe Interfaces
            22+ Built-in Parsers
            Multiple Config Sources
            Custom Parser Support
        Setting Stores
            Environment Variables
            User Secrets
            App.config/Web.config
            ChainedStore Priority
            Custom Stores
                Database
                Redis
                Azure KeyVault
        Advanced Features
            Computed Properties
                Static method invocation
                Post-load calculation
            Multi-Tenancy
                TenantKey parameter
                Tenant-aware stores
            Nested Settings
                Hierarchical configs
                Interface properties
            JSON Support
                IJsonDataType marker
                Complex object parsing
        Extensibility
            Custom Type Parsers
                ITypeParser of T
                ITypeParserRawBuilder of T
            Custom Setting Stores
                RegisterStore API
                SettingStoreMetadata
```

---

## 6. Attribute System

```mermaid
classDiagram
    class ITypeParserOptions {
        <<interface>>
        +Alias string
        +DefaultValue object
        +RawValue string
        +InputFormat string
        +Separator string
        +ProfileKey string
    }

    class OptionAttribute {
        +Alias string
        +DefaultValue object
        +RawValue string
        +InputFormat string
        +Separator string
        +ProfileKey string
    }

    class DefaultOptionAttribute {
        +ProfileKey string
    }

    class ComputedAttribute {
        +Type HelperType
        +string MethodName
    }

    ITypeParserOptions <|.. OptionAttribute

    class IAppSettings {
        <<interface>>
        +ApiUrl string
        +Timeout int
        +FullUrl string
    }

    IAppSettings ..> OptionAttribute : uses
    IAppSettings ..> DefaultOptionAttribute : uses
    IAppSettings ..> ComputedAttribute : uses

    note for IAppSettings "Example usage:\n[Option(Alias='key')]\n[DefaultOption(ProfileKey='env')]\n[Computed(typeof(Helper), 'Method')]"
```

---

## 7. Multi-Tenancy Flow

```mermaid
sequenceDiagram
    participant App as Application
    participant Cfg as MyAppCfg
    participant Store as TenantAwareStore
    participant DB as Database

    App->>Cfg: Get<ISettings>("tenant-abc")
    Cfg->>Store: GetRawValue(key, "tenant-abc")
    Store->>DB: SELECT value WHERE tenant='tenant-abc'
    DB-->>Store: "tenant-specific-value"
    Store-->>Cfg: Raw value
    Cfg-->>App: Settings for tenant-abc

    App->>Cfg: Get<ISettings>("tenant-xyz")
    Cfg->>Store: GetRawValue(key, "tenant-xyz")
    Store->>DB: SELECT value WHERE tenant='tenant-xyz'
    DB-->>Store: "different-value"
    Store-->>Cfg: Raw value
    Cfg-->>App: Settings for tenant-xyz
```

---

## 8. Complete Class Diagram

```mermaid
classDiagram
    class MyAppCfg {
        +Configure(envPrefix, secretsId)$
        +Get~T~()$ T
        +Get~T~(tenantKey)$ T
        +SettingStores$ MyAppCfgSettingStores
        +TypeParsers$ MyAppCfgTypeParsers
        +JsonSerializerSettings$ JsonSerializerSettings
    }

    class MyAppCfgSettingStores {
        -Dictionary stores
        +RegisterStore(profileKey, func)
        +Get(profileKey) Func
    }

    class MyAppCfgTypeParsers {
        -Dictionary parsers
        +Register~T~(parser)
        +Get(type) object
    }

    class AppCfgTypeMixer~TBase~ {
        +ExtendWith~T~()$ Type
        -CreateType()
        -GenerateProperties()
    }

    class ChainedStore {
        +BuildChain(envPrefix, secretsId)$ Func
        -GetFromEnvironment()
        -GetFromSecrets()
        -GetFromAppConfig()
    }

    class EnvironmentVariableStore {
        +ProfileKey string
        +GetFromEnvironmentVariables()$ string
    }

    class UserSecretsStore {
        +GetFromUserSecrets(id, key)$ string
        +ClearCache()$
    }

    class AppCfgException {
        +SettingType Type
        +PropertyName string
        +PropertyType Type
        +ParserUsed string
    }

    MyAppCfg --> MyAppCfgSettingStores
    MyAppCfg --> MyAppCfgTypeParsers
    MyAppCfg --> AppCfgTypeMixer
    MyAppCfgSettingStores --> ChainedStore
    ChainedStore --> EnvironmentVariableStore
    ChainedStore --> UserSecretsStore
    MyAppCfg ..> AppCfgException : throws
```

---

## Key Components Summary

| Component | Purpose |
|-----------|---------|
| `MyAppCfg` | Main entry point - Configure() and Get<T>() methods |
| `AppCfgTypeMixer` | Dynamic type generation via Reflection.Emit |
| `ITypeParser<T>` | Interface for parsing string values to typed values |
| `ISettingStore` | Interface for configuration sources |
| `ChainedStore` | Priority-based configuration (Env > Secrets > App.config) |
| `OptionAttribute` | Per-property configuration (alias, default, profile) |
| `DefaultOptionAttribute` | Interface-level default ProfileKey |
| `ComputedAttribute` | Calculated properties via static methods |

---

## Rendering These Diagrams

These diagrams can be rendered in:

- **GitHub/GitLab** - Automatic rendering in markdown
- **VS Code** - With Markdown Preview Mermaid Support extension
- **Mermaid Live Editor** - https://mermaid.live (export to PNG/SVG)
- **Notion, Obsidian, Confluence** - Built-in support

To export via CLI:
```bash
npm install -g @mermaid-js/mermaid-cli
mmdc -i ARCHITECTURE.md -o architecture.png
```
