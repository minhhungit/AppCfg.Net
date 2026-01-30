using AppCfg;

namespace AppCfgDemoComplete.Settings
{
    /// <summary>
    /// Demonstrates reading configuration from user secrets.
    /// User secrets file location:
    ///   Windows: %APPDATA%\Microsoft\UserSecrets\appcfg-demo-complete\secrets.json
    ///   Linux/Mac: ~/.microsoft/usersecrets/appcfg-demo-complete/secrets.json
    /// </summary>
    public interface ISecretSettings
    {
        [Option(Alias = "Secret:ApiKey")]
        string ApiKey { get; }

        [Option(Alias = "Secret:DatabasePassword")]
        string DatabasePassword { get; }

        [Option(Alias = "Secret:EncryptionKey")]
        string EncryptionKey { get; }
    }
}
