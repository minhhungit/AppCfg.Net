using AppCfg.SettingStore;
using System;
using System.Configuration;
using System.Data.SqlClient;

namespace AppCfg
{
    public partial class MyAppCfg
    {
        private static string GetRawValue(Type settingType, string tenantKey, string key, ITypeParserOptions parserOpt, ISettingStoreOptions storeOpt)
        {
            switch (storeOpt.SettingStoreType)
            {
                case SettingStoreType.AppSetting:
                    return GetRawValueForAppSetting(settingType, key);
                case SettingStoreType.MsSqlDatabase:
                    return GetRawValueForMsSqlDatabase(settingType, tenantKey, key, storeOpt.SettingStoreType, storeOpt.StoreIdentity);
                case SettingStoreType.Redis:
                    return GetRawValueForRedis(settingType, tenantKey, key, storeOpt.SettingStoreType, storeOpt.StoreIdentity);
            }

            throw new Exception($"Settting store {storeOpt.SettingStoreType} is not supported");
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

        private static string GetRawValueForMsSqlDatabase(Type settingType, string tenantKey, string settingKey, SettingStoreType settingStoreType, string storeIdentity)
        {
            if (storeIdentity == null)
            {
                storeIdentity = SettingStores.GetFirstIdentity(settingStoreType);
            }

            if (storeIdentity == null)
            {
                throw new AppCfgException("Please config for 'MSSQL Database' source: storeIdentity should not null");
            }

            if (SettingStores.Get(SettingStoreType.MsSqlDatabase, storeIdentity) is MsSqlSettingStoreConfig sourceConfig)
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
                                cmdType = System.Data.CommandType.Text;
                                try
                                {
                                    sql = string.Format(sourceConfig.QueryCmd, tenantKey, settingKey);
                                }
                                catch (Exception ex)
                                {
                                    throw new AppCfgException("Problem while building sql command text, please check your format", ex);
                                }                                
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
                                command.Parameters.Add(new SqlParameter("@appcfg_tenant_name", string.IsNullOrWhiteSpace(tenantKey) ? (object)DBNull.Value : tenantKey));                                
                                command.Parameters.Add(new SqlParameter("@appcfg_setting_name", settingKey));                                
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
                                var settingKeyMsg = string.Empty;
                                var tenantMsg = string.Empty;

                                if (!string.IsNullOrWhiteSpace(settingKey))
                                {
                                    settingKeyMsg = $"\n- SettingKey: '{settingKey}'";
                                }

                                if (!string.IsNullOrWhiteSpace(tenantKey))
                                {
                                    tenantMsg = $"\n- TenantKey: '{tenantKey}'";
                                }

                                throw new Exception($"Can not get value from database for:" +
                                    $"{settingKeyMsg}" +
                                    $"{tenantMsg}" +
                                    $"\nMake sure setting alias is correct and existed. You also should check your QueryCmd");
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
                throw new AppCfgException($"Please config for 'MSSQL Database' source: Can not load sourceConfig for storeIdentity {storeIdentity}");
            }
        }

        private static string GetRawValueForRedis(Type settingType, string tenantKey, string settingKey, SettingStoreType settingStoreType, string storeIdentity)
        {
            if (storeIdentity == null)
            {
                storeIdentity = SettingStores.GetFirstIdentity(settingStoreType) ?? string.Empty;
            }

            if (storeIdentity == null)
            {
                throw new AppCfgException("Please config for 'Redis' source: storeIdentity should not null");
            }

            

            if (SettingStores.Get(SettingStoreType.Redis, storeIdentity) is RedisSettingStoreConfig sourceConfig)
            {
                if (sourceConfig.GetRawValueFunc != null)
                {
                    return sourceConfig.GetRawValueFunc.Invoke(storeIdentity, tenantKey, settingKey);
                }
                else
                {
                    throw new Exception("Please setup method 'GetRawValueFunc(storeIdentity, tenantKey, settingKey)' for 'Redis' setting source");
                }                
            }
            else
            {
                throw new AppCfgException($"Please config for 'Redis' source: Can not load sourceConfig for storeIdentity {storeIdentity}");
            }
        }
    }
}
