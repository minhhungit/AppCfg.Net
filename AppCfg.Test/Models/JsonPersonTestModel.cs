using AppCfg.TypeParsers;
using System.Collections.Generic;

namespace AppCfg.Test
{
    public class JsonPersonTestModel : IJsonDataType
    {
        public string Title { get; }
        public PersonPropertyTestModel Properties { get; }
        public List<string> Required { get; }
    }

    public class PersonPropertyTestModel
    {
        public AgePropertyTestModel Age { get; }
    }

    public class AgePropertyTestModel
    {
        public string Description { get; }
        public string Type { get; }
        public int Minimum { get; }
    }
}
