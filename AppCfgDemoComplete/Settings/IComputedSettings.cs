using AppCfg;
using System.Collections.Generic;

namespace AppCfgDemoComplete.Settings
{
    /// <summary>
    /// Demonstrates computed settings that derive values from other configuration properties.
    ///
    /// Computed properties are not loaded from configuration sources - instead, they are
    /// calculated using static helper methods that receive the settings instance.
    /// </summary>
    public interface IComputedSettings
    {
        // Regular configuration properties (loaded from App.config)

        [Option(Alias = "Computed:Host")]
        string Host { get; }

        [Option(Alias = "Computed:Port")]
        int Port { get; }

        [Option(Alias = "Computed:Database")]
        string Database { get; }

        [Option(Alias = "Computed:Username")]
        string Username { get; }

        [Option(Alias = "Computed:UseSSL", DefaultValue = true)]
        bool UseSSL { get; }

        [Option(Alias = "Computed:MaxPoolSize", DefaultValue = 100)]
        int MaxPoolSize { get; }

        // Computed properties (calculated from other properties)

        /// <summary>
        /// Computed: Combines Host and Port into a single address
        /// </summary>
        [Computed(typeof(ComputedHelpers), nameof(ComputedHelpers.GetHostAddress))]
        string HostAddress { get; }

        /// <summary>
        /// Computed: Builds a full connection string from all database properties
        /// </summary>
        [Computed(typeof(ComputedHelpers), nameof(ComputedHelpers.BuildConnectionString))]
        string ConnectionString { get; }

        /// <summary>
        /// Computed: Gets the full database path (host:port/database)
        /// </summary>
        [Computed(typeof(ComputedHelpers), nameof(ComputedHelpers.GetFullDatabasePath))]
        string FullDatabasePath { get; }

        /// <summary>
        /// Computed: Returns a numeric value (doubled pool size)
        /// </summary>
        [Computed(typeof(ComputedHelpers), nameof(ComputedHelpers.GetEffectivePoolSize))]
        int EffectivePoolSize { get; }

        /// <summary>
        /// Computed: Returns a list of common endpoints
        /// </summary>
        [Computed(typeof(ComputedHelpers), nameof(ComputedHelpers.GetCommonEndpoints))]
        List<string> CommonEndpoints { get; }
    }

    /// <summary>
    /// Static helper class containing computation methods for IComputedSettings.
    ///
    /// Each method must:
    /// - Be public and static
    /// - Accept a single parameter of the settings interface type
    /// - Return a value matching the property type
    /// </summary>
    public static class ComputedHelpers
    {
        /// <summary>
        /// Combines Host and Port into a single address string
        /// </summary>
        public static string GetHostAddress(IComputedSettings settings)
        {
            return $"{settings.Host}:{settings.Port}";
        }

        /// <summary>
        /// Builds a connection string from multiple configuration values
        /// </summary>
        public static string BuildConnectionString(IComputedSettings settings)
        {
            var builder = new System.Text.StringBuilder();
            builder.Append($"Server={settings.Host};");
            builder.Append($"Port={settings.Port};");
            builder.Append($"Database={settings.Database};");
            builder.Append($"User={settings.Username};");
            builder.Append($"SSL={settings.UseSSL};");
            builder.Append($"MaxPoolSize={settings.MaxPoolSize};");
            return builder.ToString();
        }

        /// <summary>
        /// Returns the full database path
        /// </summary>
        public static string GetFullDatabasePath(IComputedSettings settings)
        {
            return $"{settings.Host}:{settings.Port}/{settings.Database}";
        }

        /// <summary>
        /// Calculates effective pool size (doubles the configured value for demo)
        /// </summary>
        public static int GetEffectivePoolSize(IComputedSettings settings)
        {
            // In a real scenario, this might adjust based on available resources
            return settings.MaxPoolSize * 2;
        }

        /// <summary>
        /// Returns a list of common database endpoints
        /// </summary>
        public static List<string> GetCommonEndpoints(IComputedSettings settings)
        {
            var baseUrl = $"{settings.Host}:{settings.Port}";
            return new List<string>
            {
                $"{baseUrl}/health",
                $"{baseUrl}/metrics",
                $"{baseUrl}/{settings.Database}/status"
            };
        }
    }
}
