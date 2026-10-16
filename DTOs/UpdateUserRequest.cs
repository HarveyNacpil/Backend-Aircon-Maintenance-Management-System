using System.ComponentModel.DataAnnotations;

namespace Api.DTOs
{
    public class UpdateUserRequest
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [MinLength(6)]
        public string? NewPassword { get; set; }

        public string Role { get; set; } = "Staff";
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string Status { get; set; } = "Active";
    }

    public class UpdateUsermanRequest : UpdateUserRequest { }
}