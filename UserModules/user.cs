using System.Text.Json.Serialization;

namespace Api.UserModules
{
    public sealed class User
    {
        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [JsonPropertyName("full_name")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        /// <summary>Password is included on create/update but omitted from GET responses via the endpoint layer.</summary>
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        /// <summary>Admin | Technician | Staff</summary>
        [JsonPropertyName("role")]
        public string Role { get; set; } = "Staff";

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        /// <summary>Active | Inactive</summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = "Active";

        [JsonPropertyName("store_id")]
        public int? StoreId { get; set; }

        [JsonPropertyName("created_at")]
        public string? CreatedAt { get; set; }
    }
}