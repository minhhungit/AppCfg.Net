using AppCfg;
using System.Collections.Generic;
using System.Linq;

namespace AppCfgDemo
{
    public class ListIntParser : ITypeParser<List<int>>
    {
        public List<int> Parse(string rawValue)
        {
            return rawValue.Split(',').Select(int.Parse).ToList();
        }
    }
}
