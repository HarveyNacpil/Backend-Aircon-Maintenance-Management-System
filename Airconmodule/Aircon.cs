using System.Text.Json.Serialization;

namespace Api.Airconmodule
{
    public sealed class AirconUnit
    {
        [JsonPropertyName("unit_id")]
        public int UnitId { get; set; }

        [JsonPropertyName("customer_id")]
        public int CustomerId { get; set; }

        [JsonPropertyName("brand")]
        public string? Brand { get; set; }

        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("horsepower")]
        public double? Horsepower { get; set; }

        [JsonPropertyName("serial_number")]
        public string? SerialNumber { get; set; }

        [JsonPropertyName("installation_date")]
        public string? InstallationDate { get; set; }

        [JsonPropertyName("warranty_expiry")]
        public string? WarrantyExpiry { get; set; }

        [JsonPropertyName("location")]
        public string? Location { get; set; }

        /// <summary>Active | Inactive | Under Repair</summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = "Active";
    }
}