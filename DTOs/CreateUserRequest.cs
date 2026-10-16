using System.ComponentModel.DataAnnotations;

namespace Api.DTOs
{
    public class CreateUserRequest
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public string Role { get; set; } = "Staff";
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string Status { get; set; } = "Active";
    }
}
