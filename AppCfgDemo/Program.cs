using AppCfg;
using System;

namespace AppCfgDemo
{
    public interface ISetting
    {
        [Option(DefaultValue = 99)] int DemoInt { get; }
        [Option(Alias = "long-key")] long DemoLong { get; }
        MyCustomType CustomProp { get; }
        Guid ThisIsGuid { get; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // setup custom type-parser
            MyAppCfg.TypeParserFactory.AddParser(new CustomTypeParser());

            Console.WriteLine($"DemoInt: {MyAppCfg.Get<ISetting>().DemoInt}");
            Console.WriteLine($"DemoLong: {MyAppCfg.Get<ISetting>().DemoLong}");
            Console.WriteLine($"ThisIsGuid: {MyAppCfg.Get<ISetting>().ThisIsGuid}");

            Console.WriteLine($"Custom - Name: {MyAppCfg.Get<ISetting>().CustomProp.Name}");
            Console.WriteLine($"Custom - TextLength: {MyAppCfg.Get<ISetting>().CustomProp.TextLength}");

            Console.ReadKey();
        }
    }
}
