using AppCfg;
using AppCfgDemoComplete.CustomStores;

namespace AppCfgDemoComplete.Settings
{
    /// <summary>
    /// Demonstrates multi-tenancy support.
    /// Different values can be retrieved for different tenants using MyAppCfg.Get&lt;T&gt;(tenantKey).
    ///
    /// IMPORTANT: Multi-tenancy requires a custom store that handles tenant-specific values.
    /// The [DefaultOption] attribute specifies which store to use.
    /// </summary>
    [DefaultOption(ProfileKey = InMemoryTenantStore.StoreKey)]
    public interface IMultiTenantSettings
    {
        [Option(Alias = "AppName")]
        string AppName { get; }

        [Option(Alias = "MaxConnections")]
        int MaxConnections { get; }

        [Option(Alias = "Theme")]
        string Theme { get; }

        [Option(Alias = "ApiEndpoint")]
        string ApiEndpoint { get; }
    }
}
