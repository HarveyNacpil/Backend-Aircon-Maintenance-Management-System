using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Api.DTOs;
using Api.AuthorizationModule;
using Api.UserModules;
using Api.Security;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IWebHostEnvironment _environment;

        public AuthenticationController(IUserRepository userRepository, IJwtTokenService jwtTokenService, IWebHostEnvironment environment)
        {
            _userRepository = userRepository;
            _jwtTokenService = jwtTokenService;
            _environment = environment;
        }

        private static string HashRawForSave(string rawPassword)
        {
            if (string.IsNullOrWhiteSpace(rawPassword))
            {
                throw new ArgumentException("Password is required.", nameof(rawPassword));
            }
            return PasswordHasher.Hash(rawPassword);
        }

        /// <summary>
        /// Register any account with a specified role. Admin access only.
        /// </summary>
        [HttpPost("register")]
        [Authorize(Policy = AppPolicies.AdminOnly)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { message = "Username and password are required." });
            }

            var existingUser = await _userRepository.GetByUsernameAsync(dto.Username);
            if (existingUser != null)
            {
                return Conflict(new { message = "Username already exists." });
            }

            var user = new User
            {
                Username = dto.Username,
                Password = HashRawForSave(dto.Password),
                Role = string.IsNullOrWhiteSpace(dto.Role) ? "Staff" : dto.Role,
                FullName = dto.FullName ?? dto.Username,
                Phone = dto.Phone,
                Email = dto.Email,
                Status = "Active"
            };

            await _userRepository.AddAsync(user);
            var createdUser = await _userRepository.GetByUsernameAsync(dto.Username);

            if (createdUser != null)
            {
                string token = _jwtTokenService.GenerateToken(createdUser);
                return Ok(new
                {
                    message = "User registered successfully.",
                    token,
                    userId = createdUser.UserId,
                    username = createdUser.Username,
                    role = createdUser.Role,
                });
            }

            return Ok(new { message = "User registered successfully." });
        }

        /// <summary>
        /// Public self-registration. Always assigns the regular Staff role.
        /// </summary>
        [HttpPost("register-user")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserRequest dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { message = "Username and password are required." });
            }

            var existingUser = await _userRepository.GetByUsernameAsync(dto.Username);
            if (existingUser != null)
            {
                return Conflict(new { message = "Username already exists." });
            }

            var user = new User
            {
                Username = dto.Username,
                Password = HashRawForSave(dto.Password),
                Role = "Staff",
                FullName = dto.FullName ?? dto.Username,
                Phone = dto.Phone,
                Email = dto.Email,
                Status = "Active"
            };

            await _userRepository.AddAsync(user);
            var createdUser = await _userRepository.GetByUsernameAsync(dto.Username);

            if (createdUser != null)
            {
                string token = _jwtTokenService.GenerateToken(createdUser);
                return Ok(new
                {
                    message = "Registration successful.",
                    token,
                    userId = createdUser.UserId,
                    username = createdUser.Username,
                    role = createdUser.Role,
                });
            }

            return Ok(new { message = "Registration successful." });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { message = "Username and password are required." });
            }

            var user = await _userRepository.GetByUsernameAsync(dto.Username);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            bool isValid = PasswordHasher.Verify(user.Password, dto.Password) || user.Password == dto.Password;
            if (!isValid)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            if (user.Status != "Active")
            {
                return Unauthorized(new { message = "Account is inactive." });
            }

            string token = _jwtTokenService.GenerateToken(user);
            return Ok(new
            {
                message = "Login successful.",
                token,
                userId = user.UserId,
                username = user.Username,
                role = user.Role,
            });
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username))
            {
                return BadRequest(new { message = "Username is required." });
            }

            string message = "If the account exists, a reset token has been generated.";
            var user = await _userRepository.GetByUsernameAsync(dto.Username);
            if (user == null)
            {
                return Ok(new { message });
            }

            string resetToken = _jwtTokenService.GeneratePasswordResetToken(user.UserId.ToString());
            if (_environment.IsDevelopment())
            {
                return Ok(new { message, resetToken });
            }

            return Ok(new { message });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Token) || string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                return BadRequest(new { message = "Token and new password are required." });
            }

            if (!_jwtTokenService.TryValidatePasswordResetToken(dto.Token, out string userIdText))
            {
                return BadRequest(new { message = "Invalid or expired reset token." });
            }

            if (!int.TryParse(userIdText, out var userId))
            {
                return BadRequest(new { message = "Invalid reset token format." });
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return BadRequest(new { message = "Invalid or expired reset token." });
            }

            user.Password = HashRawForSave(dto.NewPassword);
            await _userRepository.UpdateAsync(user);

            return Ok(new { message = "Password has been reset successfully." });
        }
    }
}