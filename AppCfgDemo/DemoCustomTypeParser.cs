using AppCfg.TypeParsers;

namespace AppCfgDemo
{
    public class MyCustomType
    {
        public MyCustomType(string name)
        {
            Name = name;
            TextLength = name.Length;
        }
        public string Name { get; }
        public int TextLength { get; }
    }

    public class CustomTypeParser : ITypeParser<MyCustomType>
    {
        public MyCustomType Parse(string rawValue)
        {
            return new MyCustomType("hello " + rawValue);
        }
    }
}
