# AppCfg
A mini configuration framework for .NET developers

> Note: This project is still ongoing

<br />

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
> https://github.com/minhhungit/AppCfg/tree/master/AppCfgDemo

<br />

<img src="https://raw.githubusercontent.com/minhhungit/AppCfg/master/wiki/images/demo.png" style="width: 100%;" />