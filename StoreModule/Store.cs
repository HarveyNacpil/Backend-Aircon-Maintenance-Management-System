using System.Text.Json.Serialization;

namespace Api.StoreModule
{
    public sealed class Store
    {
        [JsonPropertyName("store_id")]
        public int StoreId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        /// <summary>Open | Temporarily closed | Closed permanently</summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = "Open";

        [JsonPropertyName("is_main_branch")]
        public bool IsMainBranch { get; set; }

        // Address components
        [JsonPropertyName("street")]
        public string? Street { get; set; }

        [JsonPropertyName("barangay")]
        public string? Barangay { get; set; }

        [JsonPropertyName("city_municipality")]
        public string? CityMunicipality { get; set; }

        [JsonPropertyName("province")]
        public string? Province { get; set; }

        [JsonPropertyName("region")]
        public string? Region { get; set; }

        [JsonPropertyName("postal_code")]
        public string? PostalCode { get; set; }

        // Geographic location
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("landmark_notes")]
        public string? LandmarkNotes { get; set; }

        /// <summary>Geocoded | Pin dropped | GPS</summary>
        [JsonPropertyName("location_source")]
        public string LocationSource { get; set; } = "Pin dropped";

        [JsonPropertyName("is_verified")]
        public bool IsVerified { get; set; }

        // Contact info
        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [JsonPropertyName("mobile")]
        public string? Mobile { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("website_url")]
        public string? WebsiteUrl { get; set; }

        // Operations
        /// <summary>JSON string of hours per day & holiday note</summary>
        [JsonPropertyName("opening_hours")]
        public string? OpeningHours { get; set; }

        [JsonPropertyName("service_radius_km")]
        public double ServiceRadiusKm { get; set; } = 15.0;

        /// <summary>JSON string array of offered services</summary>
        [JsonPropertyName("services_offered")]
        public string? ServicesOffered { get; set; }

        [JsonPropertyName("accepts_walk_ins")]
        public bool AcceptsWalkIns { get; set; } = true;

        [JsonPropertyName("parking_notes")]
        public string? ParkingNotes { get; set; }

        // Staff
        [JsonPropertyName("manager_id")]
        public int? ManagerId { get; set; }

        [JsonPropertyName("manager_name")]
        public string? ManagerName { get; set; }

        /// <summary>JSON string array of assigned technician user IDs</summary>
        [JsonPropertyName("assigned_technicians")]
        public string? AssignedTechnicians { get; set; }

        // Business info
        [JsonPropertyName("business_name")]
        public string? BusinessName { get; set; }

        [JsonPropertyName("registration_number")]
        public string? RegistrationNumber { get; set; }

        [JsonPropertyName("logo_url")]
        public string? LogoUrl { get; set; }

        // Timestamps
        [JsonPropertyName("created_at")]
        public string? CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public string? UpdatedAt { get; set; }

        // Computed counters
        [JsonPropertyName("assigned_technician_count")]
        public int AssignedTechnicianCount { get; set; }

        [JsonPropertyName("assigned_customer_count")]
        public int AssignedCustomerCount { get; set; }

        [JsonPropertyName("active_request_count")]
        public int ActiveRequestCount { get; set; }
    }

    public sealed record StoreDeleteCheckResult(
        bool CanDelete,
        string Message,
        int TechnicianCount,
        int CustomerCount,
        int OpenRequestCount);
}
