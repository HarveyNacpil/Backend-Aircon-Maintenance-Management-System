using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Api.CustomerModule
{
    public sealed class CustomerAirconSummary
    {
        [JsonPropertyName("unit_id")]
        public int UnitId { get; set; }

        [JsonPropertyName("brand")]
        public string Brand { get; set; } = string.Empty;

        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("horsepower")]
        public double Horsepower { get; set; }

        [JsonPropertyName("serial_number")]
        public string? SerialNumber { get; set; }

        [JsonPropertyName("location")]
        public string? Location { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "Active";

        [JsonPropertyName("installation_date")]
        public string? InstallationDate { get; set; }

        [JsonPropertyName("warranty_expiry")]
        public string? WarrantyExpiry { get; set; }
    }

    public sealed class CustomerMaintenanceSummary
    {
        [JsonPropertyName("schedule_id")]
        public int ScheduleId { get; set; }

        [JsonPropertyName("unit_id")]
        public int UnitId { get; set; }

        [JsonPropertyName("schedule_date")]
        public string ScheduleDate { get; set; } = string.Empty;

        [JsonPropertyName("schedule_time")]
        public string? ScheduleTime { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "Scheduled";

        [JsonPropertyName("remarks")]
        public string? Remarks { get; set; }
    }

    public sealed class CustomerMapItem
    {
        [JsonPropertyName("customer_id")]
        public int CustomerId { get; set; }

        [JsonPropertyName("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [JsonPropertyName("last_name")]
        public string LastName { get; set; } = string.Empty;

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("region_code")]
        public string? RegionCode { get; set; }

        [JsonPropertyName("region_name")]
        public string? RegionName { get; set; }

        [JsonPropertyName("province_code")]
        public string? ProvinceCode { get; set; }

        [JsonPropertyName("province_name")]
        public string? ProvinceName { get; set; }

        [JsonPropertyName("city_code")]
        public string? CityCode { get; set; }

        [JsonPropertyName("city_name")]
        public string? CityName { get; set; }

        [JsonPropertyName("barangay_code")]
        public string? BarangayCode { get; set; }

        [JsonPropertyName("barangay_name")]
        public string? BarangayName { get; set; }

        [JsonPropertyName("street_address")]
        public string? StreetAddress { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("aircon_count")]
        public int AirconCount { get; set; }

        [JsonPropertyName("aircon_units")]
        public List<CustomerAirconSummary> AirconUnits { get; set; } = new();

        [JsonPropertyName("upcoming_maintenance")]
        public CustomerMaintenanceSummary? UpcomingMaintenance { get; set; }
    }
}
