using System;

namespace AppCfg
{
    public class AppCfgException : Exception
    {
        public AppCfgException()
        {
        }

        public AppCfgException(string message)
            : base(message)
        {
        }

        public AppCfgException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
