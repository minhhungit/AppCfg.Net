# AppCfg.Net [![Build status](https://ci.appveyor.com/api/projects/status/8ifb08lenlmbdf0p?svg=true)](https://ci.appveyor.com/project/minhhungit/appcfg) <a href="https://www.nuget.org/packages/AppCfg.Net/"><img src="https://img.shields.io/nuget/v/AppCfg.Net.svg?style=flat" />
A mini configuration framework for .NET developers

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

**Need demo?**
> https://github.com/minhhungit/AppCfg.Net/blob/master/AppCfgDemo/Program.cs

<br />

<img src="https://raw.githubusercontent.com/minhhungit/AppCfg/master/wiki/images/demo.png" />
