# AppCfg.Net [![Build status](https://ci.appveyor.com/api/projects/status/8ifb08lenlmbdf0p?svg=true)](https://ci.appveyor.com/project/minhhungit/appcfg) <a href="https://www.nuget.org/packages/AppCfg.Net/"><img src="https://img.shields.io/nuget/v/AppCfg.Net.svg?style=flat" /> </a>

A mini but powerful configuration framework for .NET developers

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
    long Username { get; }
}

var settings = MyAppCfg.Get<ISetting>()
var age = settings.Age;
var username = settings.Username;

```
<br />

### Supported Types
- <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/BooleanParser.cs" target="_blank">bool</a>
- <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/DecimalParser.cs" target="_blank">decimal</a>
- <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/DoubleParser.cs" target="_blank">double</a>
- <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/GuidParser.cs" target="_blank">guid</a>
- <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/IntParser.cs" target="_blank">int</a>
- <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/ListIntParser.cs" target="_blank">List&#60;int&#62;</a>
- <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/ListStringParser.cs" target="_blank">List&#60;string&#62;</a>
- <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/LongParser.cs" target="_blank">long</a>
- <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/StringParser.cs" target="_blank">string</a>
- <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/TimeSpanParser.cs" target="_blank">TimeSpan</a>
- <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/DateTimeParser.cs" target="_blank">DateTime</a>
- <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/JsonParser.cs" target="_blank">Json</a>
- <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/ConnectionStringParser.cs" target="_blank">ConnectionString</a>

### Supported Setting Store
- AppSetting ^^
- MsSqlDatabase

### Custom Type Parser
> In case you need a type parser which was not supported, even if you don't like existed parser, you can create one for yourself and **register** it, or create a request for me, I will try to help!

- The simplest way to create a custom parser is implementing parser from interface **ITypeParser**, see <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfg/TypeParsers/IntParser.cs" target="_blank">example</a>. 
Default setting source is `appSetting`
- If you want to change setting source, like you want to get setting from a json file, or even from table in database, you should implement your parser from interface ITypeParserRawBuilder, demo is <a href="https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfgDemo/CustomParsers/DemoParserWithRawBuilder.cs" target="_blank">here</a>

After you created a parser, you must register it, for example:
> MyAppCfg.TypeParsers.Register(new DemoParserWithRawBuilder<JsonPerson>());

<br />

<img src="https://raw.githubusercontent.com/minhhungit/AppCfg/master/wiki/images/demo.png" />
