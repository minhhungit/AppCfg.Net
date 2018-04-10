﻿using System;

namespace AppCfg
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class OptionAttribute : Attribute
    {
        /// <summary>
        /// Alias is used to override option name if it's stored by a different name in external stores
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Set to override the default value if option is not found in any stores
        /// </summary>
        public object DefaultValue { get; set; }

    }
}
