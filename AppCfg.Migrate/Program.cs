using AppCfg.SettingStore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AppCfg.Migrate
{
    /// <summary>
    /// appcfg-migrate: copy appSettings / connectionStrings from App.config or Web.config into secrets.json.
    /// Thin CLI over <see cref="UserSecretsMigrator"/>.
    /// </summary>
    internal static class Program
    {
        private const int ExitOk = 0;
        private const int ExitUsage = 1;
        private const int ExitFailed = 2;

        private static int Main(string[] args)
        {
            CliOptions options;
            try
            {
                options = CliOptions.Parse(args);
            }
            catch (CliUsageException ex)
            {
                Console.Error.WriteLine("error: " + ex.Message);
                Console.Error.WriteLine();
                PrintUsage(Console.Error);
                return ExitUsage;
            }

            if (options.ShowHelp)
            {
                PrintUsage(Console.Out);
                return ExitOk;
            }

            try
            {
                var migrationOptions = new MigrationOptions
                {
                    IncludeConnectionStrings = !options.SkipConnectionStrings,
                    OverwriteExisting = options.Overwrite,
                    DryRun = options.DryRun,
                    KeyFilter = options.BuildKeyFilter()
                };

                var isJson = string.Equals(Path.GetExtension(options.ConfigPath), ".json", StringComparison.OrdinalIgnoreCase);
                var result = isJson
                    ? UserSecretsMigrator.MigrateFromJsonFile(options.ConfigPath, options.UserSecretsId, migrationOptions)
                    : UserSecretsMigrator.MigrateFromConfigFile(options.ConfigPath, options.UserSecretsId, migrationOptions);

                Report(options, result);
                return ExitOk;
            }
            catch (AppCfgException ex)
            {
                Console.Error.WriteLine("error: " + ex.Message);
                return ExitFailed;
            }
        }

        private static void Report(CliOptions options, MigrationResult result)
        {
            if (options.DryRun)
            {
                Console.WriteLine("DRY RUN - nothing was written.");
            }

            Console.WriteLine($"Secrets file : {result.SecretsFilePath}");
            Console.WriteLine($"Migrated     : {result.MigratedKeys.Count}");
            foreach (var key in result.MigratedKeys)
            {
                Console.WriteLine($"  + {key}");
            }

            if (result.SkippedKeys.Count > 0)
            {
                Console.WriteLine($"Kept existing: {result.SkippedKeys.Count}  (use --overwrite to replace)");
                foreach (var key in result.SkippedKeys)
                {
                    Console.WriteLine($"  = {key}");
                }
            }

            Console.WriteLine();
            Console.WriteLine("Next steps:");
            Console.WriteLine($"  1. Remove the migrated keys from {Path.GetFileName(options.ConfigPath)} so they leave source control.");
            Console.WriteLine($"  2. At startup call:  MyAppCfg.Configure(envVarPrefix: \"MYAPP__\", userSecretsId: \"{options.UserSecretsId}\");");
            Console.WriteLine("  3. Your [Option(Alias = ...)] keys stay the same - secrets.json now overrides App.config.");
        }

        private static void PrintUsage(TextWriter writer)
        {
            writer.WriteLine("appcfg-migrate - move App.config / Web.config / appsettings.json settings into secrets.json");
            writer.WriteLine();
            writer.WriteLine("Usage:");
            writer.WriteLine("  appcfg-migrate <config-file> --id <user-secrets-id> [options]");
            writer.WriteLine();
            writer.WriteLine("  <config-file> is App.config, Web.config, a standalone <appSettings> file (*.config),");
            writer.WriteLine("  or a .NET Core JSON file (appsettings*.json, detected by the .json extension).");
            writer.WriteLine();
            writer.WriteLine("Options:");
            writer.WriteLine("  --id <id>                 User secrets ID (the value you pass to MyAppCfg.Configure). Required.");
            writer.WriteLine("  -i, --include <pattern>   Only migrate keys matching this wildcard (repeatable). Default: all keys.");
            writer.WriteLine("  -e, --exclude <pattern>   Skip keys matching this wildcard (repeatable).");
            writer.WriteLine("      --no-connection-strings   Do not migrate <connectionStrings>.");
            writer.WriteLine("      --overwrite           Replace values that already exist in secrets.json.");
            writer.WriteLine("  -n, --dry-run             Show what would be migrated without writing.");
            writer.WriteLine("  -h, --help                Show this help.");
            writer.WriteLine();
            writer.WriteLine("Examples:");
            writer.WriteLine("  appcfg-migrate .\\Web.config --id my-app-secrets --dry-run");
            writer.WriteLine("  appcfg-migrate .\\App.config --id my-app-secrets -i \"*Password*\" -i \"*Key\" -i \"*Secret*\"");
            writer.WriteLine("  appcfg-migrate .\\appsettings.Development.json --id my-app-secrets -i \"ConnectionStrings:*\" -i \"*Secret*\"");
            writer.WriteLine();
            writer.WriteLine("Secrets file location:");
            writer.WriteLine("  Windows   %APPDATA%\\Microsoft\\UserSecrets\\<id>\\secrets.json");
            writer.WriteLine("  Linux/Mac ~/.microsoft/usersecrets/<id>/secrets.json");
        }
    }

    internal sealed class CliUsageException : Exception
    {
        public CliUsageException(string message) : base(message) { }
    }

    internal sealed class CliOptions
    {
        public string ConfigPath { get; private set; }
        public string UserSecretsId { get; private set; }
        public List<string> Include { get; } = new List<string>();
        public List<string> Exclude { get; } = new List<string>();
        public bool SkipConnectionStrings { get; private set; }
        public bool Overwrite { get; private set; }
        public bool DryRun { get; private set; }
        public bool ShowHelp { get; private set; }

        public static CliOptions Parse(string[] args)
        {
            var o = new CliOptions();

            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                switch (arg)
                {
                    case "-h":
                    case "--help":
                    case "/?":
                        o.ShowHelp = true;
                        return o;
                    case "--id":
                        o.UserSecretsId = NextValue(args, ref i, arg);
                        break;
                    case "-i":
                    case "--include":
                        o.Include.Add(NextValue(args, ref i, arg));
                        break;
                    case "-e":
                    case "--exclude":
                        o.Exclude.Add(NextValue(args, ref i, arg));
                        break;
                    case "--no-connection-strings":
                        o.SkipConnectionStrings = true;
                        break;
                    case "--overwrite":
                        o.Overwrite = true;
                        break;
                    case "-n":
                    case "--dry-run":
                        o.DryRun = true;
                        break;
                    default:
                        if (arg.StartsWith("-"))
                        {
                            throw new CliUsageException($"Unknown option '{arg}'.");
                        }
                        if (o.ConfigPath != null)
                        {
                            throw new CliUsageException($"Unexpected argument '{arg}'. Only one config file can be given.");
                        }
                        o.ConfigPath = arg;
                        break;
                }
            }

            if (string.IsNullOrWhiteSpace(o.ConfigPath))
            {
                throw new CliUsageException("A config file path is required.");
            }
            if (string.IsNullOrWhiteSpace(o.UserSecretsId))
            {
                throw new CliUsageException("--id <user-secrets-id> is required.");
            }

            return o;
        }

        /// <summary>Returns null when no filters are given, so the library migrates everything.</summary>
        public Func<string, bool> BuildKeyFilter()
        {
            if (Include.Count == 0 && Exclude.Count == 0)
            {
                return null;
            }

            var include = Include.Select(ToRegex).ToList();
            var exclude = Exclude.Select(ToRegex).ToList();

            return key =>
                (include.Count == 0 || include.Any(r => r.IsMatch(key)))
                && !exclude.Any(r => r.IsMatch(key));
        }

        private static Regex ToRegex(string wildcard)
        {
            var pattern = "^" + Regex.Escape(wildcard).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
            return new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        private static string NextValue(string[] args, ref int i, string option)
        {
            if (i + 1 >= args.Length || args[i + 1].StartsWith("-"))
            {
                throw new CliUsageException($"Option '{option}' requires a value.");
            }
            return args[++i];
        }
    }
}
