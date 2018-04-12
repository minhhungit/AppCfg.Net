using AppCfg;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

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

    public interface IConnectionStringSetting
    {
        [Option(Alias = "myConnFirst")]
        SqlConnectionStringBuilder First { get; }
        [Option(Alias = "myConnSecond")]
        SqlConnectionStringBuilder Second { get; }
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

            Console.WriteLine($"DemoBoolean: {MySettings.BaseSettings.DemoBoolean}");
            Console.WriteLine($"DemoDateTime: {MySettings.BaseSettings.DemoDateTime}");
            Console.WriteLine($"DemoDateTimeWithFormat: {MySettings.BaseSettings.DemoDateTimeWithFormat:MMM dd, yyyy}");
            Console.WriteLine($"DemoDecimal: {MySettings.BaseSettings.DemoDecimal}");
            Console.WriteLine($"DemoDouble: {MySettings.BaseSettings.DemoDouble}");
            Console.WriteLine($"DemoGuid: {MySettings.BaseSettings.DemoGuid}");
            Console.WriteLine($"DemoInt: {MySettings.BaseSettings.DemoInt}");
            Console.WriteLine($"DemoLong: {MySettings.BaseSettings.DemoLong}");
            Console.WriteLine($"DemoString: {MySettings.BaseSettings.DemoString}");
            Console.WriteLine($"DemoTimeSpanFirst: {MySettings.BaseSettings.DemoTimeSpanFirst}");
            Console.WriteLine($"DemoTimeSpanSecond: {MySettings.BaseSettings.DemoTimeSpanSecond}\n");

            Console.WriteLine($"Numbers:");
            foreach (var num in MySettings.BaseSettings.Numbers)
            {
                Console.WriteLine($"   + {num}");
            }

            Console.WriteLine($"\nAnimal - Name: {MySettings.JsonSettings.CuteAnimal.Name}");
            Console.WriteLine($"Animal - Legs: {MySettings.JsonSettings.CuteAnimal.Legs}");
            Console.WriteLine($"Animal - CanSwim: {MySettings.JsonSettings.CuteAnimal.CanSwim}");
            Console.WriteLine($"Animal - SampleDay: {MySettings.JsonSettings.CuteAnimal.SampleDay:MMM dd, yyyy}\n");

            Console.WriteLine($"Machine - DayWithNewFormat: {MySettings.JsonSettings.Optimus.DayWithNewFormat:MM-dd-yyyy}\n");

            Console.WriteLine($"ConnnString - First: {MySettings.ConnSettings.First.ConnectionString}");
            Console.WriteLine($"ConnnString - Second: InitialCatalog = {MySettings.ConnSettings.Second.InitialCatalog} | ConnectTimeout = {MySettings.ConnSettings.Second.ConnectTimeout}");

            Console.ReadKey();
        }
    }
}
