namespace AppCfg.TypeParsers
{
    public interface ITypeParser<T>
    {
        T Parse(string rawValue);
    }
}
