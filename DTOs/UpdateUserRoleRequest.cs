using System.ComponentModel.DataAnnotations;

namespace Api.DTOs
{
    public class CreateUserRoleRequest
    {
        [Required]
        [StringLength(100)]
        public string UserRole { get; set; } = string.Empty;
    }

    public class UpdateUserroleRequest
    {
        [Required]
        [StringLength(100)]
        public string UserRole { get; set; } = string.Empty;
    }
}