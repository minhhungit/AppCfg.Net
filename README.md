# AppCfg
A mini configuration framework for .NET developers

> Note: This project is still ongoing
<br /><br />

```xml
<appSettings>
  <add key="DemoInt" value="1"/>
  <add key="long-key" value="9223372036854775807"/>
</appSettings>
```
<br />

```csharp
public interface ISetting
{
    int DemoInt { get; }
    long DemoLong { get; }
    Guid ThisIsGuid { get; }
}

var myIntValue = MyAppCfg.Get<ISetting>().DemoInt;
var myLongValue = MyAppCfg.Get<ISetting>().DemoLong;

```
