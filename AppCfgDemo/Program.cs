using AppCfg;
using System;

namespace AppCfgDemo
{
    public interface ISetting
    {
        [Option(DefaultValue = 99)] int DemoInt { get; }
        [Option(Alias = "long-key")] long DemoLong { get; }
        [Option(Alias = "author")] Person Person { get; }
        Guid ThisIsGuid { get; }
    }

    public interface IJsonSetting
    {
        [Option(Alias = "cute_animal")] Animal CuteAnimal { get; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // setup custom typeparser
            TypeParserFactory.AddParser(new PersonParser());

            Console.WriteLine($"DemoInt: {MyAppCfg.Get<ISetting>().DemoInt}");
            Console.WriteLine($"DemoLong: {MyAppCfg.Get<ISetting>().DemoLong}");
            Console.WriteLine($"ThisIsGuid: {MyAppCfg.Get<ISetting>().ThisIsGuid}\n");

            Console.WriteLine($"Custom - Name: {MyAppCfg.Get<ISetting>().Person.Name}");
            Console.WriteLine($"Custom - Age: {MyAppCfg.Get<ISetting>().Person.Age}");
            Console.WriteLine($"Custom - Birthday: {MyAppCfg.Get<ISetting>().Person.Birthday:yyyy-MM-dd}");
            Console.WriteLine($"Custom - IsMarried: {MyAppCfg.Get<ISetting>().Person.IsMarried}\n");

            Console.WriteLine($"Animal - Name: {MyAppCfg.Get<IJsonSetting>().CuteAnimal.Name}");
            Console.WriteLine($"Animal - Legs: {MyAppCfg.Get<IJsonSetting>().CuteAnimal.Legs}");
            Console.WriteLine($"Animal - CanSwim: {MyAppCfg.Get<IJsonSetting>().CuteAnimal.CanSwim}");
            Console.WriteLine($"Animal - SampleDay: {MyAppCfg.Get<IJsonSetting>().CuteAnimal.SampleDay:MMM dd, yyyy}");

            Console.ReadKey();
        }
    }
}
