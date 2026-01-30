using System;

namespace AppCfg
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class OptionAttribute : Attribute, ITypeParserOptions
    {
        /// <summary>
        /// Alias is used to override option name if it's stored by a different name in external stores
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Set to override the default value if option is not found in any stores
        /// </summary>
        public object DefaultValue { get; set; }
        public string RawValue { get; set; }

        public string InputFormat { get; set; }

        public string Separator { get; set; }

        /// <summary>
        /// Profile key to identify which store to use.
        /// If null or empty, settings will be loaded from App.config/Web.config (AppSettings/ConnectionStrings)
        /// If specified at property level, it overrides the interface-level DefaultOption
        /// </summary>
        public string ProfileKey { get; set; }
    }
}
