using AppCfg;
using System;
using System.Collections.Generic;

namespace AppCfgDemoComplete.Settings
{
    public interface IBasicSettings
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

        // List collections with custom separators
        [Option(Separator = "^")]
        List<int> Numbers { get; }

        [Option(Separator = ";")]
        List<string> StringList { get; }

        [Option(Separator = "|")]
        List<Guid> GuidList { get; }

        [Option(Separator = ",")]
        List<bool> BoolList { get; }

        [Option(Separator = ";")]
        List<DateTime> DateTimeList { get; }

        // Enum examples
        [Option(Alias = "enum_by_int")]
        DemoEnum EnumByInt { get; }

        [Option(Alias = "enum_by_string")]
        DemoEnum EnumByString { get; }

        [Option(Alias = "hello_enum")]
        SecondDemoEnum HelloEnum { get; }
    }

    public enum DemoEnum
    {
        FirstValue = 0,
        SecondValue = 1,
        ThirdValue = 2
    }

    public enum SecondDemoEnum
    {
        Hello = 0,
        World = 1
    }
}
