using AppCfg;
using AppCfgDemoComplete.CustomParsers;
using AppCfgDemoComplete.CustomStores;
using AppCfgDemoComplete.Models;
using System;
using System.Configuration;

namespace AppCfgDemoComplete
{
    /// <summary>
    /// Central configuration initialization class.
    /// Demonstrates different initialization strategies for various scenarios.
    /// </summary>
    public static class MySettings
    {
        private static bool _isBasicInitialized = false;
        private static bool _isDatabaseInitialized = false;
        private static bool _isRedisInitialized = false;
        private static bool _isTenantStoreInitialized = false;

        /// <summary>
        /// Basic initialization - registers custom parsers and JSON settings.
        /// Call this for most demos that don't require special stores.
        /// </summary>
        public static void InitializeBasic()
        {
            if (_isBasicInitialized) return;

            // Register custom type parsers
            // By default, AppCfg auto-creates JsonParser for IJsonDataType at runtime
            // But if you want to use a custom parser, register it here
            MyAppCfg.TypeParsers.Register(new DemoParserWithRawBuilder<JsonPerson>());
            MyAppCfg.TypeParsers.Register(new DemoParserWithRawBuilder<JsonHelloWorld>());

            // Setup JSON serializer settings
            MyAppCfg.JsonSerializerSettings = new Newtonsoft.Json.JsonSerializerSettings
            {
                DateFormatString = "dd+MM+yyyy" // Default format for dates
            };

            _isBasicInitialized = true;
        }

        /// <summary>
        /// Initialize ChainedStore for priority-based configuration.
        /// This is the RECOMMENDED approach for production applications.
        /// Priority order: Environment Variables → User Secrets → App.config
        /// </summary>
        public static void InitializeChainedStore()
        {
            InitializeBasic();

            // One-liner configuration with ChainedStore
            // This sets up: Environment Variables → User Secrets → AppSettings
            MyAppCfg.Configure(
                envVarPrefix: "APPCFG__",
                userSecretsId: "appcfg-demo-complete"
            );
        }

        /// <summary>
        /// Initialize in-memory tenant store for multi-tenancy demo.
        /// </summary>
        public static void InitializeTenantStore()
        {
            if (_isTenantStoreInitialized) return;

            InitializeBasic();
            InMemoryTenantStore.Register();
            _isTenantStoreInitialized = true;
        }

        /// <summary>
        /// Initialize database custom store.
        /// Demonstrates how to register a custom store for SQL Server.
        /// </summary>
        public static void InitializeDatabaseStore()
        {
            InitializeBasic();

            if (!IsDatabaseEnabled())
            {
                return;
            }

            try
            {
                MssqlStore.Register();
                _isDatabaseInitialized = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize database store: {ex.Message}");
            }
        }

        /// <summary>
        /// Initialize Redis custom store.
        /// Demonstrates how to register a custom store for Redis.
        /// </summary>
        public static void InitializeRedisStore()
        {
            InitializeBasic();

            if (!IsRedisEnabled())
            {
                return;
            }

            try
            {
                RedisStore.Register();
                _isRedisInitialized = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize Redis store: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if database feature is enabled in configuration.
        /// </summary>
        public static bool IsDatabaseEnabled()
        {
            var setting = ConfigurationManager.AppSettings["Demo:EnableDatabase"];
            return !string.IsNullOrEmpty(setting) && setting.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Check if Redis feature is enabled in configuration.
        /// </summary>
        public static bool IsRedisEnabled()
        {
            var setting = ConfigurationManager.AppSettings["Demo:EnableRedis"];
            return !string.IsNullOrEmpty(setting) && setting.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Check if database store was successfully initialized.
        /// </summary>
        public static bool IsDatabaseInitialized()
        {
            return _isDatabaseInitialized;
        }

        /// <summary>
        /// Check if Redis store was successfully initialized.
        /// </summary>
        public static bool IsRedisInitialized()
        {
            return _isRedisInitialized;
        }

        /// <summary>
        /// Reset all initialization flags (useful for testing).
        /// </summary>
        public static void Reset()
        {
            _isBasicInitialized = false;
            _isDatabaseInitialized = false;
            _isRedisInitialized = false;
            _isTenantStoreInitialized = false;
        }
    }
}
