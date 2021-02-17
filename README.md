# AppCfg.Net [![Build status](https://ci.appveyor.com/api/projects/status/8ifb08lenlmbdf0p?svg=true)](https://ci.appveyor.com/project/minhhungit/appcfg) <a href="https://www.nuget.org/packages/AppCfg.Net/"><img src="https://img.shields.io/nuget/v/AppCfg.Net.svg?style=flat" /> </a>

Type-safe, easy and power configuration framework for .NET developers

### Installation
> Install-Package AppCfg.Net

### Demo 
<a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfgDemo/Program.cs">https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfgDemo/Program.cs</a>
<br />

### First look

```xml
<appSettings>
  <add key="Age" value="29"/>
  <add key="user-name" value="Hung Vo"/>
</appSettings>
```
<br />

```csharp
public interface ISetting
{
    int Age { get; }
	
    [Option(Alias = "user-name")] 
    string Username { get; }
}

var settings = MyAppCfg.Get<ISetting>()

var age = settings.Age;
var username = settings.Username;

```
<br />

>Did you notice? Settings are read-only!

### Supported Types
- <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/BooleanParser.cs" target="_blank">bool</a> and <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/ListBooleanParser.cs" target="_blank">List&#60;bool&#62;</a>
- <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/DecimalParser.cs" target="_blank">decimal</a> and <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/ListDecimalParser.cs" target="_blank">List&#60;decimal&#62;</a>
- <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/DoubleParser.cs" target="_blank">double</a> and <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/ListDoubleParser.cs" target="_blank">List&#60;double&#62;</a>
- <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/GuidParser.cs" target="_blank">guid</a> and <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/ListGuidParser.cs" target="_blank">List&#60;guid&#62;</a>
- <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/IntParser.cs" target="_blank">int</a> and <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/ListIntParser.cs" target="_blank">List&#60;int&#62;</a>
- <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/LongParser.cs" target="_blank">long</a> and <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/ListLongParser.cs" target="_blank">List&#60;long&#62;</a>
- <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/StringParser.cs" target="_blank">string</a> and <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/ListStringParser.cs" target="_blank">List&#60;string&#62;</a>
- <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/TimeSpanParser.cs" target="_blank">TimeSpan</a> and <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/ListTimespanParser.cs" target="_blank">List&#60;TimeSpan&#62;</a>
- <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/DateTimeParser.cs" target="_blank">DateTime</a> and <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/ListDateTimeParser.cs" target="_blank">List&#60;DateTime&#62;</a>
- <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/JsonParser.cs" target="_blank">Json</a>
- <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/EnumParser.cs" target="_blank">Enum</a>
- <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/ConnectionStringParser.cs" target="_blank">ConnectionString</a>

> AppCfg.Net is also support nested setting

### Supported Setting Store
- AppSetting [ <a href="https://github.com/minhhungit/AppCfg.Net/tree/master/AppCfgDemo" target="_blank">Demo</a> ]
- Custom stores like MSSQL database, Redis, Text file... it's up to you. [ <a href="https://github.com/minhhungit/AppCfg.Net/tree/master/AppCfgDemoMssql" target="_blank">Demo</a> ]

> With Custom stores, it also support multi-tenancy!

### Custom Type Parser
> In case you need a type parser which was not supported, even if you don't like existed parser, you can create one for yourself and **register** it, or create a request for me, I will try to help!

- The simplest way to create a custom parser is implementing parser from interface **ITypeParser**, see <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/IntParser.cs" target="_blank">example</a>. 
Default setting source is `appSetting`
- If you want to change setting source, like you want to get setting from a json file, or even from table in database, you should implement your parser from interface ITypeParserRawBuilder, demo is <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfgDemo/CustomParsers/DemoParserWithRawBuilder.cs" target="_blank">here</a>

After you created a parser, you must register it, for example:
> MyAppCfg.TypeParsers.Register(new DemoParserWithRawBuilder<JsonPerson>());

<br />

<img src="https://raw.githubusercontent.com/minhhungit/AppCfg/master/wiki/images/demo.png" />


### Donate ^^
**If you like my works and would like to support then you can buy me a coffee ☕️ anytime**

<a href='https://ko-fi.com/I2I13GAGL' target='_blank'><img height='36' style='border:0px;height:36px;' src='https://cdn.ko-fi.com/cdn/kofi4.png?v=2' border='0' alt='Buy Me a Coffee at ko-fi.com' /></a> 

**I would appreciate it!!!**
