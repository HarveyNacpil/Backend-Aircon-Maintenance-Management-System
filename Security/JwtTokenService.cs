using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Api.UserModules;

namespace Api.Security
{
    public interface IJwtTokenService
    {
        string GenerateToken(User user);
        string GeneratePasswordResetToken(string userId);
        bool TryValidatePasswordResetToken(string token, out string userId);
    }

    public class JwtTokenService : IJwtTokenService
    {
        private const string PasswordResetTokenType = "password_reset";

        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int    _expirationMinutes;
        private readonly int    _resetExpirationMinutes;

        public JwtTokenService(IConfiguration configuration, IHostEnvironment environment)
        {
            _secretKey              = JwtConfiguration.ResolveSigningKey(configuration, environment);
            _issuer                 = configuration["Jwt:Issuer"]            ?? JwtConfiguration.DefaultIssuer;
            _audience               = configuration["Jwt:Audience"]          ?? JwtConfiguration.DefaultAudience;
            _expirationMinutes      = int.TryParse(configuration["Jwt:ExpirationMinutes"],      out int exp)   ? exp   : JwtConfiguration.DefaultAccessExpirationMinutes;
            _resetExpirationMinutes = int.TryParse(configuration["Jwt:ResetExpirationMinutes"], out int reset) ? reset : JwtConfiguration.DefaultResetExpirationMinutes;
        }

        public string GenerateToken(User user)
        {
            var key = Encoding.UTF8.GetBytes(_secretKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name,            user.Username),
                new Claim("user_role",                user.Role),          // Admin | Technician | Staff
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var descriptor = new SecurityTokenDescriptor
            {
                Subject            = new ClaimsIdentity(claims),
                Expires            = DateTime.UtcNow.AddMinutes(_expirationMinutes),
                Issuer             = _issuer,
                Audience           = _audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(handler.CreateToken(descriptor));
        }

        public string GeneratePasswordResetToken(string userId)
        {
            var key = Encoding.UTF8.GetBytes(_secretKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim("token_type",              PasswordResetTokenType),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var descriptor = new SecurityTokenDescriptor
            {
                Subject            = new ClaimsIdentity(claims),
                Expires            = DateTime.UtcNow.AddMinutes(_resetExpirationMinutes),
                Issuer             = _issuer,
                Audience           = _audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(handler.CreateToken(descriptor));
        }

        public bool TryValidatePasswordResetToken(string token, out string userId)
        {
            userId = string.Empty;
            if (string.IsNullOrWhiteSpace(token)) return false;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var key     = Encoding.UTF8.GetBytes(_secretKey);

                var principal = handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer              = _issuer,
                    ValidAudience            = _audience,
                    IssuerSigningKey         = new SymmetricSecurityKey(key),
                    ClockSkew                = TimeSpan.FromMinutes(1)
                }, out _);

                string tokenType = principal.FindFirst("token_type")?.Value ?? string.Empty;
                if (!string.Equals(tokenType, PasswordResetTokenType, StringComparison.Ordinal))
                    return false;

                userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
                return !string.IsNullOrWhiteSpace(userId);
            }
            catch
            {
                return false;
            }
        }
    }
}
