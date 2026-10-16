using System.Text.Json.Serialization;

namespace Api.CustomerModule
{
    public sealed class Customer
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
        public double? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double? Longitude { get; set; }

        [JsonPropertyName("store_id")]
        public int? StoreId { get; set; }

        [JsonPropertyName("created_at")]
        public string? CreatedAt { get; set; }
    }
}
