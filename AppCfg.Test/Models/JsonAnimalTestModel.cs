using AppCfg.TypeParsers;
using System;

namespace AppCfg.Test
{
    public class JsonAnimalTestModel : IJsonDataType
    {
        public string Name { get; }
        public int Legs { get; }
        public bool CanSwim { get;  }
        public DateTime SampleDay { get; }
    }
}
