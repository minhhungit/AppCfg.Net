using AppCfg;
using System;
using System.Collections.Generic;

namespace AppCfgDemo
{
    public interface ISetting
    {
        bool DemoBoolean { get; }
        DateTime DemoDateTime { get; }

        [Option(InputFormat = "dd+MM/yyyy")]
        DateTime DemoDateTimeWithFormat { get; }

        decimal DemoDecimal { get; }
        double DemoDouble { get; }
        Guid DemoGuid { get; }

        [Option(DefaultValue = 77)]
        int DemoInt { get; }

        [Option(Alias = "long-key")]
        long DemoLong { get; }

        string DemoString { get; }
        TimeSpan DemoTimeSpanFirst { get; }
        TimeSpan DemoTimeSpanSecond { get; }

        [Option(Separator = "^")]
        List<int> Numbers { get; }
    }

    public interface IJsonSetting
    {
        [Option(Alias = "cute_animal")]
        Animal CuteAnimal { get; }

        [Option(Alias = "test_setting_with_js_config")]
        Machine Optimus { get; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Use a wrapper class to initial settings, 
            // It's also help us see errors if something wrong before it dives into hell
            // Anyway, this is just a demo
            MySettings.Init();

            Console.WriteLine($"DemoBoolean: {MySettings.First.DemoBoolean}");
            Console.WriteLine($"DemoDateTime: {MySettings.First.DemoDateTime}");
            Console.WriteLine($"DemoDateTimeWithFormat: {MySettings.First.DemoDateTimeWithFormat:MMM dd, yyyy}");
            Console.WriteLine($"DemoDecimal: {MySettings.First.DemoDecimal}");
            Console.WriteLine($"DemoDouble: {MySettings.First.DemoDouble}");
            Console.WriteLine($"DemoGuid: {MySettings.First.DemoGuid}");
            Console.WriteLine($"DemoInt: {MySettings.First.DemoInt}");
            Console.WriteLine($"DemoLong: {MySettings.First.DemoLong}");
            Console.WriteLine($"DemoString: {MySettings.First.DemoString}");
            Console.WriteLine($"DemoTimeSpanFirst: {MySettings.First.DemoTimeSpanFirst}");
            Console.WriteLine($"DemoTimeSpanSecond: {MySettings.First.DemoTimeSpanSecond}\n");

            Console.WriteLine($"Numbers:");
            foreach (var num in MySettings.First.Numbers)
            {
                Console.WriteLine($"   + {num}");
            }

            Console.WriteLine($"\nAnimal - Name: {MySettings.Second.CuteAnimal.Name}");
            Console.WriteLine($"Animal - Legs: {MySettings.Second.CuteAnimal.Legs}");
            Console.WriteLine($"Animal - CanSwim: {MySettings.Second.CuteAnimal.CanSwim}");
            Console.WriteLine($"Animal - SampleDay: {MySettings.Second.CuteAnimal.SampleDay:MMM dd, yyyy}\n");

            Console.WriteLine($"Machine - DayWithNewFormat: {MySettings.Second.Optimus.DayWithNewFormat:MM-dd-yyyy}");

            Console.ReadKey();
        }
    }
}
