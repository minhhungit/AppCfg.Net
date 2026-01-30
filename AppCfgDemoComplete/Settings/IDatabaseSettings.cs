using AppCfg;
using AppCfgDemoComplete.CustomStores;
using System;

namespace AppCfgDemoComplete.Settings
{
    /// <summary>
    /// Demonstrates custom database store with SQL Server.
    /// Requires Database feature to be enabled and store to be registered.
    /// </summary>
    public interface IDatabaseSettings
    {
        [Option(Alias = "Author", ProfileKey = MssqlStore.StoreKey_CommandText)]
        string AuthorFromCommandText { get; }

        [Option(Alias = "NO-SETTING", ProfileKey = MssqlStore.StoreKey_CommandText, DefaultValue = "Default Author Value")]
        string AuthorDefaultValue { get; }

        [Option(Alias = "PartnerKey", ProfileKey = MssqlStore.StoreKey_StoredProc)]
        Guid PartnerKeyFromStoredProc { get; }
    }
}
