namespace AppCfg
{
    public interface ITypeParser<T>
    {
        T Parse(string rawValue);
    }
}
