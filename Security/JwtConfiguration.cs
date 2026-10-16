using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Api.Security
{
    public static class JwtConfiguration
    {
        public const int    MinJwtKeyLength                = 32;
        public const string DefaultIssuer                 = "aircon-api";
        public const string DefaultAudience               = "aircon-client";
        public const int    DefaultAccessExpirationMinutes = 480; // 8 hours
        public const int    DefaultResetExpirationMinutes  = 15;
        public const string DevelopmentFallbackKey         = "LocalDevelopmentJwtKey-ReplaceBeforeProduction-2026";

        public static string ResolveSigningKey(IConfiguration configuration, IHostEnvironment environment)
        {
            string? configuredKey = configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(configuredKey))
            {
                if (environment.IsDevelopment())
                    return DevelopmentFallbackKey;

                throw new InvalidOperationException(
                    $"Jwt:Key is required in non-development environments (min {MinJwtKeyLength} chars).");
            }

            if (configuredKey.Length < MinJwtKeyLength)
                throw new InvalidOperationException($"Jwt:Key must be at least {MinJwtKeyLength} characters.");

            return configuredKey;
        }
    }
}
