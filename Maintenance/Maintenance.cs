using System.Text.Json.Serialization;

namespace Api.Maintenance
{
    public sealed class MaintenanceSchedule
    {
        [JsonPropertyName("schedule_id")]
        public int ScheduleId { get; set; }

        [JsonPropertyName("customer_id")]
        public int CustomerId { get; set; }

        [JsonPropertyName("unit_id")]
        public int UnitId { get; set; }

        [JsonPropertyName("technician_id")]
        public int TechnicianId { get; set; }

        [JsonPropertyName("schedule_date")]
        public string ScheduleDate { get; set; } = string.Empty;

        [JsonPropertyName("schedule_time")]
        public string ScheduleTime { get; set; } = string.Empty;

        /// <summary>Scheduled | Completed | Cancelled</summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = "Scheduled";

        [JsonPropertyName("remarks")]
        public string? Remarks { get; set; }
    }

    public sealed class MaintenanceRecord
    {
        [JsonPropertyName("record_id")]
        public int RecordId { get; set; }

        [JsonPropertyName("schedule_id")]
        public int ScheduleId { get; set; }

        [JsonPropertyName("work_performed")]
        public string? WorkPerformed { get; set; }

        [JsonPropertyName("parts_replaced")]
        public string? PartsReplaced { get; set; }

        [JsonPropertyName("refrigerant_added")]
        public string? RefrigerantAdded { get; set; }

        [JsonPropertyName("technician_notes")]
        public string? TechnicianNotes { get; set; }

        [JsonPropertyName("next_service_date")]
        public string? NextServiceDate { get; set; }

        [JsonPropertyName("completed_date")]
        public string? CompletedDate { get; set; }
    }
}
