# AppCfg.Net [![Build status](https://ci.appveyor.com/api/projects/status/8ifb08lenlmbdf0p?svg=true)](https://ci.appveyor.com/project/minhhungit/appcfg) <a href="https://www.nuget.org/packages/AppCfg.Net/"><img src="https://img.shields.io/nuget/v/AppCfg.Net.svg?style=flat" />
A mini but powerful configuration framework for .NET developers

### Installation
> Install-Package AppCfg.Net


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

var age = MyAppCfg.Get<ISetting>().Age;
var username = MyAppCfg.Get<ISetting>().Username;

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

> In case you need a type parser which was not supported or even you don't like existed parser, you can create one for yourself and **replace** existed one, or create a request!

**Need demo?**
> Link: https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfgDemo/Program.cs

<br /><br />

<img src="https://raw.githubusercontent.com/minhhungit/AppCfg/master/wiki/images/demo.png" />
