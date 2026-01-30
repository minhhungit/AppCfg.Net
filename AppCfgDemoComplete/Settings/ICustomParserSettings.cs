using AppCfg;
using AppCfgDemoComplete.Models;

namespace AppCfgDemoComplete.Settings
{
    public interface ICustomParserSettings
    {
        [Option(Alias = "test-json-file")]
        JsonPerson DemoRawBuilder { get; }

        [Option(Alias = "hello-world")]
        JsonHelloWorld HelloWorld { get; }
    }
}
