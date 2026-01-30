using AppCfg.TypeParsers;
using System;

namespace AppCfgDemoComplete.Models
{
    public class Animal : IJsonDataType
    {
        public string Name { get; }
        public int Legs { get; }
        public bool CanSwim { get;  }
        public DateTime SampleDay { get; }
    }
}
