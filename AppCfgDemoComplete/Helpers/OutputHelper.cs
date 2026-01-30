using System;

namespace AppCfgDemoComplete.Helpers
{
    public static class OutputHelper
    {
        public static void WriteSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ {message}");
            Console.ResetColor();
        }

        public static void WriteError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ {message}");
            Console.ResetColor();
        }

        public static void WriteInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"ℹ {message}");
            Console.ResetColor();
        }

        public static void WriteWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⚠ {message}");
            Console.ResetColor();
        }

        public static void WriteSetting(string key, object value, string source = "App.config")
        {
            Console.Write("  ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{key}: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{value}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($" [{source}]");
            Console.ResetColor();
        }

        public static void WriteHeader(string text)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"\n{text}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(new string('─', text.Length));
            Console.ResetColor();
        }

        public static void MaskSecret(string secret)
        {
            if (string.IsNullOrEmpty(secret) || secret.Length <= 4)
            {
                Console.Write("****");
                return;
            }

            Console.Write(secret.Substring(0, 2));
            Console.Write(new string('*', secret.Length - 4));
            Console.Write(secret.Substring(secret.Length - 2));
        }

        public static void WriteSection(string title)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"═══ {title} ═══");
            Console.ResetColor();
        }
    }
}
