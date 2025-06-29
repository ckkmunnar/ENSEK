namespace ApiTestFramework.Utilities
{
    /// <summary>
    /// Simple utility to load environment variables from .env file
    /// </summary>
    public static class EnvironmentLoader
    {
        /// <summary>
        /// Loads environment variables from .env file if it exists
        /// </summary>
        /// <param name="envFilePath">Path to .env file (default: .env in current directory)</param>
        public static void LoadFromFile(string envFilePath = ".env")
        {
            if (!File.Exists(envFilePath))
            {
                return; // .env file is optional
            }

            try
            {
                var lines = File.ReadAllLines(envFilePath);
                
                foreach (var line in lines)
                {
                    // Skip empty lines and comments
                    if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                        continue;

                    // Parse KEY=VALUE format
                    var parts = line.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        var key = parts[0].Trim();
                        var value = parts[1].Trim();
                        
                        // Remove quotes if present
                        if (value.StartsWith("\"") && value.EndsWith("\""))
                        {
                            value = value.Substring(1, value.Length - 2);
                        }
                        
                        // Always set the value from .env file (prefer .env over system env)
                        Environment.SetEnvironmentVariable(key, value);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Warning: Could not load .env file: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads environment variables from .env file, automatically finding it in the project root
        /// </summary>
        public static void LoadDotEnvFile()
        {
            // Try absolute path first (for when running from test runner)
            var envPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".env");
            
            if (File.Exists(envPath))
            {
                LoadFromFile(envPath);
                return;
            }
            
            // Fallback to relative path
            if (File.Exists(".env"))
            {
                LoadFromFile(".env");
                return;
            }
            
            // If neither found, that's okay - .env is optional
        }

        /// <summary>
        /// Gets an environment variable with a fallback default value
        /// </summary>
        /// <param name="key">Environment variable key</param>
        /// <param name="defaultValue">Default value if not found</param>
        /// <returns>Environment variable value or default</returns>
        public static string GetEnvironmentVariable(string key, string defaultValue = "")
        {
            return Environment.GetEnvironmentVariable(key) ?? defaultValue;
        }

        /// <summary>
        /// Shows basic ENSEK configuration
        /// </summary>
        public static void ShowConfiguration()
        {
            // Ensure .env is loaded first
            LoadDotEnvFile();
            
            System.Console.WriteLine("ENSEK Configuration:");
            System.Console.WriteLine($"  Base URL: {GetEnvironmentVariable("ENSEK_BASE_URL", "https://qacandidatetest.ensek.io")}");
            System.Console.WriteLine($"  Username: {GetEnvironmentVariable("ENSEK_USERNAME", "test")}");
            System.Console.WriteLine($"  Auth Token: [CONFIGURED]");
            System.Console.WriteLine($"  Password: [CONFIGURED]");
        }
    }
}
