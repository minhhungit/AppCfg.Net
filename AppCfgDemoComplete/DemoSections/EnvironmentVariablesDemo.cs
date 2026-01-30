using AppCfg;
using AppCfgDemoComplete.Helpers;
using AppCfgDemoComplete.Settings;
using System;

namespace AppCfgDemoComplete.DemoSections
{
    public static class EnvironmentVariablesDemo
    {
        public static void Run()
        {
            MenuHelper.ShowSectionHeader(
                "Environment Variables Demo",
                "Demonstrates reading configuration from environment variables.\n" +
                "Environment variables take precedence over App.config when using ChainedStore.\n" +
                "Variable format: APPCFG__Section__Key (double underscore separator)"
            );

            try
            {
                MySettings.InitializeChainedStore();
                var settings = MyAppCfg.Get<IEnvironmentSettings>();

                OutputHelper.WriteHeader("Environment Variable Settings");
                OutputHelper.WriteInfo("Demo auto-initialized these env vars on startup:\n");
                OutputHelper.WriteSetting("SimpleString", settings.SimpleString, "ENV VAR (auto-set by demo)");
                OutputHelper.WriteSetting("IntValue", settings.IntValue, "ENV VAR (auto-set by demo)");

                Console.WriteLine();
                OutputHelper.WriteSection("How to Test");
                Console.WriteLine("Set environment variables to override App.config values:");
                Console.WriteLine();
                Console.WriteLine("Windows (Command Prompt):");
                Console.WriteLine("  set APPCFG__Env__SimpleString=from-environment");
                Console.WriteLine("  set APPCFG__Env__IntValue=999");
                Console.WriteLine();
                Console.WriteLine("Windows (PowerShell):");
                Console.WriteLine("  $env:APPCFG__Env__SimpleString=\"from-environment\"");
                Console.WriteLine("  $env:APPCFG__Env__IntValue=\"999\"");
                Console.WriteLine();
                Console.WriteLine("Linux/Mac:");
                Console.WriteLine("  export APPCFG__Env__SimpleString=from-environment");
                Console.WriteLine("  export APPCFG__Env__IntValue=999");
                Console.WriteLine();
                OutputHelper.WriteInfo("Environment variables are perfect for containerized deployments!");
            }
            catch (Exception ex)
            {
                OutputHelper.WriteError($"Failed to run EnvironmentVariables demo: {ex.Message}");
            }

            MenuHelper.PauseForUser();
        }
    }
}
