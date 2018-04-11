namespace AppCfg
{
    public interface ITypeParser<T>
    {
        T Parse(string rawValue, string inputFormat = null, string separator = null);
    }
}
