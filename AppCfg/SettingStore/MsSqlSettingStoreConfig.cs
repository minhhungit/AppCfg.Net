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
        /// </summary>
        public string QueryCmd { get; set; }

        /// <summary>
        /// Specifies how a QueryCmd is interpreted
        /// </summary>
        public QueryCmdType QueryCmdType { get; set; }
    }
}
