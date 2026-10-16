using System.Text.Json.Serialization;

namespace Api.ServiceRequestModule
{
    public sealed class ServiceRequest
    {
        [JsonPropertyName("request_id")]
        public int RequestId { get; set; }

        [JsonPropertyName("customer_id")]
        public int CustomerId { get; set; }

        [JsonPropertyName("unit_id")]
        public int UnitId { get; set; }

        [JsonPropertyName("issue_description")]
        public string IssueDescription { get; set; } = string.Empty;

        /// <summary>High | Medium | Low</summary>
        [JsonPropertyName("priority")]
        public string Priority { get; set; } = "Medium";

        /// <summary>Open | Assigned | In Progress | Completed</summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = "Open";

        [JsonPropertyName("assigned_technician")]
        public int? AssignedTechnician { get; set; }

        [JsonPropertyName("store_id")]
        public int? StoreId { get; set; }

        [JsonPropertyName("request_date")]
        public string? RequestDate { get; set; }
    }
}
