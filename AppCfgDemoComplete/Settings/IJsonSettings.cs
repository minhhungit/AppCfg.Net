using AppCfg;
using AppCfgDemoComplete.Models;

namespace AppCfgDemoComplete.Settings
{
    public interface IJsonSettings
    {
        [Option(Alias = "cute_animal")]
        Animal CuteAnimal { get; }

        [Option(Alias = "test_setting_with_js_config")]
        Machine Optimus { get; }
    }
}
