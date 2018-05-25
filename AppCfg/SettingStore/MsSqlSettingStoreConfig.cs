namespace AppCfg.SettingStore
{
    public enum QueryCmdType
    {
        Text,
        StoreProcedure
    }

    public class MsSqlSettingStoreConfig
    {
        public string ConnectionString { get; set; }

        /// <summary>
        /// A sql text command or the name of a store procedure
        /// If you use CommandText, index {0} is settingKey, use index {1} for tenantKey for multi-tenant feature
        /// If you use StoredProcedure, your stored should have [@appcfg_setting_name] parameter, add [@appcfg_tenant_name] for multi-tenant feature
        /// </summary>
        public string QueryCmd { get; set; }

        /// <summary>
        /// Specifies how a QueryCmd is interpreted
        /// </summary>
        public QueryCmdType QueryCmdType { get; set; }
    }
}
