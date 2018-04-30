using AppCfg.SettingStore;
using System;
using System.Configuration;
using System.Data.SqlClient;

namespace AppCfg
{
    public partial class MyAppCfg
    {
        private static string GetRawValue(Type settingType, string key, ITypeParserOptions options)
        {
            switch (options.SettingStoreType)
            {
                case SettingStoreType.AppConfig:
                    return GetRawValueForAppSetting(settingType, key);
                case SettingStoreType.MsSqlDatabase:
                    return GetRawValueForMsSqlDatabase(settingType, key);
            }

            throw new Exception($"Settting store {options.SettingStoreType} is not supported");
        }

        private static string GetRawValueForAppSetting(Type settingType, string key)
        {
            if (settingType == typeof(SqlConnectionStringBuilder))
            {
                return ConfigurationManager.ConnectionStrings[key]?.ConnectionString;
            }
            else
            {
                return ConfigurationManager.AppSettings[key];
            }
        }

        private static string GetRawValueForMsSqlDatabase(Type settingType, string key)
        {
            var sourceConfig = SettingStores.Get(StoresSupportedType.MsSqlDatabase) as MsSqlSettingStoreConfig;
            if (sourceConfig != null)
            {
                if (string.IsNullOrWhiteSpace(sourceConfig.ConnectionString))
                {
                    throw new AppCfgException("Please setup connectionstring in UseMsSqlDataSource() method");
                }
                if (string.IsNullOrWhiteSpace(sourceConfig.QueryCmd))
                {
                    throw new AppCfgException("Please setup QueryCmd in UseMsSqlDataSource() method");
                }

                try
                {
                    using (SqlConnection connection = new SqlConnection(sourceConfig.ConnectionString))
                    {
                        connection.Open();
                        var sql = string.Empty;
                        var cmdType = System.Data.CommandType.Text;

                        switch (sourceConfig.QueryCmdType)
                        {
                            case QueryCmdType.Text:
                                sql = string.Format(sourceConfig.QueryCmd, key);
                                cmdType = System.Data.CommandType.Text;
                                break;
                            case QueryCmdType.StoreProcedure:
                                sql = sourceConfig.QueryCmd;
                                cmdType = System.Data.CommandType.StoredProcedure;
                                break;
                        }

                        using (SqlCommand command = new SqlCommand(sql, connection))
                        {
                            command.CommandType = cmdType;
                            if (sourceConfig.QueryCmdType == QueryCmdType.StoreProcedure)
                            {
                                command.Parameters.Add(new SqlParameter("@appcfg_setting_name", key));
                            }
                            var reader = command.ExecuteReader();

                            string tmpVal = null;
                            var hasValue = false;
                            while (reader.Read())
                            {
                                hasValue = true;
                                tmpVal = reader.GetString(0);
                            }
                            if (!hasValue)
                            {
                                throw new Exception($"Can not get value for setting '{key}' from database. Make sure setting alias is correct and existed. You also should check your QueryCmd");
                            }

                            return tmpVal;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new AppCfgException("Has problem when AppCfg try to get setting from database", ex);
                }
            }
            else
            {
                throw new AppCfgException($"Please registry config for setting source 'MsSqlDatabase'");
            }
        }
    }
}
