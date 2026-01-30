using System;

namespace AppCfg
{
    /// <summary>
    /// Specifies default ProfileKey for all properties in an interface.
    /// Individual properties can override this default using [Option] attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class DefaultOptionAttribute : Attribute
    {
        /// <summary>
        /// Default profile key for all properties in this interface.
        /// If null or empty, settings will be loaded from App.config/Web.config (AppSettings/ConnectionStrings)
        /// Individual properties can override this using [Option(ProfileKey = "...")]
        /// </summary>
        public string ProfileKey { get; set; }
    }
}
