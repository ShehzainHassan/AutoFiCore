namespace AutoFiCore.Utilities
{
    public static class StartupValidator
    {
        public static void ValidateEnvironment(IConfiguration config, IWebHostEnvironment env)
        {
            if (env.IsProduction())
            {
                ValidateProductionSecrets(config);
                ValidateSecuritySettings(config);
                ValidatePerformanceSettings(config);
            }
        }

        private static void ValidateProductionSecrets(IConfiguration config)
        {
            var requiredEnvVars = new[] { "DATABASE_CONNECTION_STRING", "JWT_SECRET" };

            var missing = requiredEnvVars
                .Where(v => string.IsNullOrEmpty(Environment.GetEnvironmentVariable(v)))
                .ToList();

            if (missing.Any())
            {
                throw new InvalidOperationException($"Missing production environment variables: {string.Join(", ", missing)}");
            }
        }

        private static void ValidateSecuritySettings(IConfiguration config)
        {
            var jwtSecret = config["Jwt:Secret"];

            if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32)
            {
                throw new InvalidOperationException("Security Check Failed: JWT:Secret must be at least 32 characters long.");
            }
        }

        private static void ValidatePerformanceSettings(IConfiguration config)
        {
            int maxRetryCount = config.GetValue<int>("DatabaseSettings:MaxRetryCount");
            int retryDelaySeconds = config.GetValue<int>("DatabaseSettings:RetryDelaySeconds");
            int commandTimeoutSeconds = config.GetValue<int>("DatabaseSettings:CommandTimeoutSeconds");

            if (maxRetryCount < 3)
            {
                throw new InvalidOperationException("Performance Check Failed: DatabaseSettings:MaxRetryCount must be at least 3.");
            }

            if (retryDelaySeconds > 10)
            {
                throw new InvalidOperationException("Performance Check Failed: DatabaseSettings:RetryDelaySeconds must not exceed 10 seconds.");
            }

            if (commandTimeoutSeconds > 60)
            {
                throw new InvalidOperationException("Performance Check Failed: DatabaseSettings:CommandTimeoutSeconds must not exceed 60 seconds.");
            }
        }
    }
}
