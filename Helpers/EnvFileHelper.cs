using DotNetEnv;
using System;
using System.IO;

namespace Helpers
{
    public static class EnvFileHelper
    {
        static EnvFileHelper()
        {
            var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            if (env != "Test") // Ignora o carregamento se estiver em ambiente de teste
            {
                LoadEnvFile();
            }
        }

        private static void LoadEnvFile()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var envFilePath = Path.Combine(currentDirectory, ".env");

            if (!File.Exists(envFilePath))
            {
                // Tenta carregar a partir do diretório pai
                string? parentDirectory = Directory.GetParent(currentDirectory)?.FullName;
                if (parentDirectory != null) envFilePath = Path.Combine(parentDirectory, ".env");

                if (!File.Exists(envFilePath))
                {
                    throw new FileNotFoundException("The .env file could not be found in the current or parent directory.");
                }
            }

            Console.WriteLine($"Loaded .env from: {envFilePath}");
            Env.Load(envFilePath);
        }

        public static string GetString(string key)
        {
            var value = Environment.GetEnvironmentVariable(key);
            if (value == null)
            {
                throw new InvalidOperationException($"Environment variable '{key}' is not set.");
            }

            return value;
        }
    }
}