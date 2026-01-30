using System;

namespace AppCfg
{
    /// <summary>
    /// Indicates that a property value should be computed using a static method
    /// instead of loaded from configuration.
    ///
    /// The target method must be static and have the signature:
    ///   static TReturn MethodName(TSettings settings)
    ///
    /// where TReturn matches the property type and TSettings is the interface type.
    /// </summary>
    /// <example>
    /// <code>
    /// public interface IDatabaseSettings
    /// {
    ///     [Option(Alias = "DB:Host")]
    ///     string Host { get; }
    ///
    ///     [Computed(typeof(DatabaseHelpers), nameof(DatabaseHelpers.BuildConnectionString))]
    ///     string FullConnectionString { get; }
    /// }
    ///
    /// public static class DatabaseHelpers
    /// {
    ///     public static string BuildConnectionString(IDatabaseSettings settings)
    ///     {
    ///         return $"Server={settings.Host};Port={settings.Port}";
    ///     }
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ComputedAttribute : Attribute
    {
        /// <summary>
        /// The type containing the static computation method
        /// </summary>
        public Type HelperType { get; }

        /// <summary>
        /// The name of the static method that performs the computation
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// Creates a computed property attribute
        /// </summary>
        /// <param name="helperType">Type containing the static computation method</param>
        /// <param name="methodName">Name of the static method to invoke</param>
        public ComputedAttribute(Type helperType, string methodName)
        {
            HelperType = helperType ?? throw new ArgumentNullException(nameof(helperType));
            MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
        }
    }
}
