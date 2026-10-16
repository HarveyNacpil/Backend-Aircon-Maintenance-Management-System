using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Api.Main;

namespace Api.StoreModule
{
    public class StoreRepository : BaseRepository, IStoreRepository
    {
        public StoreRepository(MyCon dbConnection) : base(dbConnection) { }

        private const string BaseSelect = @"
            SELECT 
                s.store_id, s.name, s.code, s.status, s.is_main_branch,
                s.street, s.barangay, s.city_municipality, s.province, s.region, s.postal_code,
                s.latitude, s.longitude, s.landmark_notes, s.location_source, s.is_verified,
                s.phone, s.mobile, s.email, s.website_url,
                s.opening_hours, s.service_radius_km, s.services_offered, s.accepts_walk_ins, s.parking_notes,
                s.manager_id, u.full_name AS manager_name, s.assigned_technicians,
                s.business_name, s.registration_number, s.logo_url,
                s.created_at, s.updated_at,
                (SELECT COUNT(*) FROM users tech WHERE tech.role = 'Technician' AND tech.store_id = s.store_id) AS tech_count,
                (SELECT COUNT(*) FROM customers c WHERE c.store_id = s.store_id) AS cust_count,
                (SELECT COUNT(*) FROM service_requests r WHERE r.store_id = s.store_id AND r.status != 'Completed') AS open_req_count
            FROM stores s
            LEFT JOIN users u ON s.manager_id = u.user_id";

        private Store MapReader(DbDataReader r)
        {
            var store = new Store
            {
                StoreId           = ReadValue<int>(r,     "store_id", 0),
                Name              = ReadValue<string>(r,  "name", string.Empty),
                Code              = ReadValue<string>(r,  "code", string.Empty),
                Status            = ReadValue<string>(r,  "status", "Open"),
                IsMainBranch      = ReadValue<int>(r,     "is_main_branch", 0) == 1,
                Street            = ReadValue<string?>(r, "street", null),
                Barangay          = ReadValue<string?>(r, "barangay", null),
                CityMunicipality  = ReadValue<string?>(r, "city_municipality", null),
                Province          = ReadValue<string?>(r, "province", null),
                Region            = ReadValue<string?>(r, "region", null),
                PostalCode        = ReadValue<string?>(r, "postal_code", null),
                Latitude          = ReadValue<double>(r,  "latitude", 0.0),
                Longitude         = ReadValue<double>(r,  "longitude", 0.0),
                LandmarkNotes     = ReadValue<string?>(r, "landmark_notes", null),
                LocationSource    = ReadValue<string>(r,  "location_source", "Pin dropped"),
                IsVerified        = ReadValue<int>(r,     "is_verified", 0) == 1,
                Phone             = ReadValue<string?>(r, "phone", null),
                Mobile            = ReadValue<string?>(r, "mobile", null),
                Email             = ReadValue<string?>(r, "email", null),
                WebsiteUrl        = ReadValue<string?>(r, "website_url", null),
                OpeningHours      = ReadValue<string?>(r, "opening_hours", null),
                ServiceRadiusKm   = ReadValue<double>(r,  "service_radius_km", 15.0),
                ServicesOffered   = ReadValue<string?>(r, "services_offered", null),
                AcceptsWalkIns    = ReadValue<int>(r,     "accepts_walk_ins", 1) == 1,
                ParkingNotes      = ReadValue<string?>(r, "parking_notes", null),
                ManagerId         = ReadValue<int?>(r,    "manager_id", null),
                ManagerName       = ReadValue<string?>(r, "manager_name", null),
                AssignedTechnicians = ReadValue<string?>(r, "assigned_technicians", null),
                BusinessName      = ReadValue<string?>(r, "business_name", null),
                RegistrationNumber = ReadValue<string?>(r, "registration_number", null),
                LogoUrl           = ReadValue<string?>(r, "logo_url", null),
                CreatedAt         = ReadValue<string?>(r, "created_at", null),
                UpdatedAt         = ReadValue<string?>(r, "updated_at", null),
                AssignedTechnicianCount = ReadValue<int>(r, "tech_count", 0),
                AssignedCustomerCount   = ReadValue<int>(r, "cust_count", 0),
                ActiveRequestCount      = ReadValue<int>(r, "open_req_count", 0),
            };

            // If JSON assigned_technicians has entries, combine or fallback for technician count
            if (store.AssignedTechnicianCount == 0 && !string.IsNullOrWhiteSpace(store.AssignedTechnicians))
            {
                try
                {
                    var parsed = System.Text.Json.JsonSerializer.Deserialize<int[]>(store.AssignedTechnicians);
                    if (parsed != null && parsed.Length > 0)
                    {
                        store.AssignedTechnicianCount = parsed.Length;
                    }
                }
                catch { }
            }

            return store;
        }

        public async Task<IEnumerable<Store>> GetAllAsync()
        {
            string sql = $"{BaseSelect} ORDER BY s.is_main_branch DESC, s.name ASC";
            return await ExecuteReaderToListAsync(sql, MapReader);
        }

        public async Task<Store?> GetByIdAsync(int storeId)
        {
            string sql = $"{BaseSelect} WHERE s.store_id = @sid";
            var list = await ExecuteReaderToListAsync(sql, MapReader, new[] { CreateParameter("sid", storeId) });
            return list.FirstOrDefault();
        }

        public async Task<int> AddAsync(Store entity)
        {
            // If this is the very first store, make it the main branch by default
            long existingCount = await ExecuteScalarAsync<long>("SELECT COUNT(*) FROM stores");
            bool isMain = entity.IsMainBranch || existingCount == 0;

            if (isMain)
            {
                await ExecuteNonQueryAsync("UPDATE stores SET is_main_branch = 0");
            }

            const string sql = @"
                INSERT INTO stores (
                    name, code, status, is_main_branch,
                    street, barangay, city_municipality, province, region, postal_code,
                    latitude, longitude, landmark_notes, location_source, is_verified,
                    phone, mobile, email, website_url,
                    opening_hours, service_radius_km, services_offered, accepts_walk_ins, parking_notes,
                    manager_id, assigned_technicians,
                    business_name, registration_number, logo_url
                ) VALUES (
                    @name, @code, @status, @isMainBranch,
                    @street, @barangay, @cityMunicipality, @province, @region, @postalCode,
                    @latitude, @longitude, @landmarkNotes, @locationSource, @isVerified,
                    @phone, @mobile, @email, @websiteUrl,
                    @openingHours, @serviceRadiusKm, @servicesOffered, @acceptsWalkIns, @parkingNotes,
                    @managerId, @assignedTechnicians,
                    @businessName, @registrationNumber, @logoUrl
                );
                SELECT last_insert_rowid();";

            long id = await ExecuteScalarAsync<long>(sql, new[]
            {
                CreateParameter("name", entity.Name),
                CreateParameter("code", entity.Code),
                CreateParameter("status", entity.Status),
                CreateParameter("isMainBranch", isMain ? 1 : 0),
                CreateParameter("street", (object?)entity.Street ?? DBNull.Value),
                CreateParameter("barangay", (object?)entity.Barangay ?? DBNull.Value),
                CreateParameter("cityMunicipality", (object?)entity.CityMunicipality ?? DBNull.Value),
                CreateParameter("province", (object?)entity.Province ?? DBNull.Value),
                CreateParameter("region", (object?)entity.Region ?? DBNull.Value),
                CreateParameter("postalCode", (object?)entity.PostalCode ?? DBNull.Value),
                CreateParameter("latitude", entity.Latitude),
                CreateParameter("longitude", entity.Longitude),
                CreateParameter("landmarkNotes", (object?)entity.LandmarkNotes ?? DBNull.Value),
                CreateParameter("locationSource", entity.LocationSource),
                CreateParameter("isVerified", entity.IsVerified ? 1 : 0),
                CreateParameter("phone", (object?)entity.Phone ?? DBNull.Value),
                CreateParameter("mobile", (object?)entity.Mobile ?? DBNull.Value),
                CreateParameter("email", (object?)entity.Email ?? DBNull.Value),
                CreateParameter("websiteUrl", (object?)entity.WebsiteUrl ?? DBNull.Value),
                CreateParameter("openingHours", (object?)entity.OpeningHours ?? DBNull.Value),
                CreateParameter("serviceRadiusKm", entity.ServiceRadiusKm),
                CreateParameter("servicesOffered", (object?)entity.ServicesOffered ?? DBNull.Value),
                CreateParameter("acceptsWalkIns", entity.AcceptsWalkIns ? 1 : 0),
                CreateParameter("parkingNotes", (object?)entity.ParkingNotes ?? DBNull.Value),
                CreateParameter("managerId", (object?)entity.ManagerId ?? DBNull.Value),
                CreateParameter("assignedTechnicians", (object?)entity.AssignedTechnicians ?? DBNull.Value),
                CreateParameter("businessName", (object?)entity.BusinessName ?? DBNull.Value),
                CreateParameter("registrationNumber", (object?)entity.RegistrationNumber ?? DBNull.Value),
                CreateParameter("logoUrl", (object?)entity.LogoUrl ?? DBNull.Value),
            });

            // If technicians were designated in assignedTechnicians JSON array, update users.store_id
            if (!string.IsNullOrWhiteSpace(entity.AssignedTechnicians))
            {
                try
                {
                    var techIds = System.Text.Json.JsonSerializer.Deserialize<int[]>(entity.AssignedTechnicians);
                    if (techIds != null)
                    {
                        foreach (var tid in techIds)
                        {
                            await ExecuteNonQueryAsync("UPDATE users SET store_id = @sid WHERE user_id = @uid", new[]
                            {
                                CreateParameter("sid", (int)id),
                                CreateParameter("uid", tid)
                            });
                        }
                    }
                }
                catch { }
            }

            return (int)id;
        }

        public async Task UpdateAsync(Store entity)
        {
            if (entity.IsMainBranch)
            {
                await ExecuteNonQueryAsync("UPDATE stores SET is_main_branch = 0 WHERE store_id != @sid",
                    new[] { CreateParameter("sid", entity.StoreId) });
            }

            const string sql = @"
                UPDATE stores SET
                    name = @name,
                    code = @code,
                    status = @status,
                    is_main_branch = @isMainBranch,
                    street = @street,
                    barangay = @barangay,
                    city_municipality = @cityMunicipality,
                    province = @province,
                    region = @region,
                    postal_code = @postalCode,
                    latitude = @latitude,
                    longitude = @longitude,
                    landmark_notes = @landmarkNotes,
                    location_source = @locationSource,
                    is_verified = @isVerified,
                    phone = @phone,
                    mobile = @mobile,
                    email = @email,
                    website_url = @websiteUrl,
                    opening_hours = @openingHours,
                    service_radius_km = @serviceRadiusKm,
                    services_offered = @servicesOffered,
                    accepts_walk_ins = @acceptsWalkIns,
                    parking_notes = @parkingNotes,
                    manager_id = @managerId,
                    assigned_technicians = @assignedTechnicians,
                    business_name = @businessName,
                    registration_number = @registrationNumber,
                    logo_url = @logoUrl,
                    updated_at = CURRENT_TIMESTAMP
                WHERE store_id = @sid";

            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("sid", entity.StoreId),
                CreateParameter("name", entity.Name),
                CreateParameter("code", entity.Code),
                CreateParameter("status", entity.Status),
                CreateParameter("isMainBranch", entity.IsMainBranch ? 1 : 0),
                CreateParameter("street", (object?)entity.Street ?? DBNull.Value),
                CreateParameter("barangay", (object?)entity.Barangay ?? DBNull.Value),
                CreateParameter("cityMunicipality", (object?)entity.CityMunicipality ?? DBNull.Value),
                CreateParameter("province", (object?)entity.Province ?? DBNull.Value),
                CreateParameter("region", (object?)entity.Region ?? DBNull.Value),
                CreateParameter("postalCode", (object?)entity.PostalCode ?? DBNull.Value),
                CreateParameter("latitude", entity.Latitude),
                CreateParameter("longitude", entity.Longitude),
                CreateParameter("landmarkNotes", (object?)entity.LandmarkNotes ?? DBNull.Value),
                CreateParameter("locationSource", entity.LocationSource),
                CreateParameter("isVerified", entity.IsVerified ? 1 : 0),
                CreateParameter("phone", (object?)entity.Phone ?? DBNull.Value),
                CreateParameter("mobile", (object?)entity.Mobile ?? DBNull.Value),
                CreateParameter("email", (object?)entity.Email ?? DBNull.Value),
                CreateParameter("websiteUrl", (object?)entity.WebsiteUrl ?? DBNull.Value),
                CreateParameter("openingHours", (object?)entity.OpeningHours ?? DBNull.Value),
                CreateParameter("serviceRadiusKm", entity.ServiceRadiusKm),
                CreateParameter("servicesOffered", (object?)entity.ServicesOffered ?? DBNull.Value),
                CreateParameter("acceptsWalkIns", entity.AcceptsWalkIns ? 1 : 0),
                CreateParameter("parkingNotes", (object?)entity.ParkingNotes ?? DBNull.Value),
                CreateParameter("managerId", (object?)entity.ManagerId ?? DBNull.Value),
                CreateParameter("assignedTechnicians", (object?)entity.AssignedTechnicians ?? DBNull.Value),
                CreateParameter("businessName", (object?)entity.BusinessName ?? DBNull.Value),
                CreateParameter("registrationNumber", (object?)entity.RegistrationNumber ?? DBNull.Value),
                CreateParameter("logoUrl", (object?)entity.LogoUrl ?? DBNull.Value),
            });

            // Synchronize users.store_id if technicians were updated
            if (!string.IsNullOrWhiteSpace(entity.AssignedTechnicians))
            {
                try
                {
                    var techIds = System.Text.Json.JsonSerializer.Deserialize<int[]>(entity.AssignedTechnicians);
                    if (techIds != null)
                    {
                        foreach (var tid in techIds)
                        {
                            await ExecuteNonQueryAsync("UPDATE users SET store_id = @sid WHERE user_id = @uid", new[]
                            {
                                CreateParameter("sid", entity.StoreId),
                                CreateParameter("uid", tid)
                            });
                        }
                    }
                }
                catch { }
            }
        }

        public async Task<StoreDeleteCheckResult> CheckCanDeleteAsync(int storeId)
        {
            var p = new[] { CreateParameter("sid", storeId) };

            long custCount = await ExecuteScalarAsync<long>(
                "SELECT COUNT(*) FROM customers WHERE store_id = @sid", p);

            long openReqCount = await ExecuteScalarAsync<long>(
                "SELECT COUNT(*) FROM service_requests WHERE store_id = @sid AND status != 'Completed'", p);

            long techCount = await ExecuteScalarAsync<long>(
                "SELECT COUNT(*) FROM users WHERE role = 'Technician' AND store_id = @sid", p);

            // Also check assigned_technicians in store JSON
            var store = await GetByIdAsync(storeId);
            if (store != null && !string.IsNullOrWhiteSpace(store.AssignedTechnicians))
            {
                try
                {
                    var ids = System.Text.Json.JsonSerializer.Deserialize<int[]>(store.AssignedTechnicians);
                    if (ids != null && ids.Length > techCount)
                    {
                        techCount = ids.Length;
                    }
                }
                catch { }
            }

            int tCount = (int)techCount;
            int cCount = (int)custCount;
            int rCount = (int)openReqCount;

            bool hasBlockers = tCount > 0 || cCount > 0 || rCount > 0;
            if (hasBlockers)
            {
                var reasons = new List<string>();
                if (tCount > 0) reasons.Add($"{tCount} assigned {(tCount == 1 ? "technician" : "technicians")}");
                if (cCount > 0) reasons.Add($"{cCount} {(cCount == 1 ? "customer" : "customers")}");
                if (rCount > 0) reasons.Add($"{rCount} active service {(rCount == 1 ? "request" : "requests")}");

                string msg = $"Cannot delete this branch because it still has {string.Join(", ", reasons)}. Reassign them to another branch first.";
                return new StoreDeleteCheckResult(false, msg, tCount, cCount, rCount);
            }

            return new StoreDeleteCheckResult(true, "Store can be deleted safely.", 0, 0, 0);
        }

        public async Task DeleteAsync(int storeId)
        {
            await ExecuteNonQueryAsync("DELETE FROM stores WHERE store_id = @sid",
                new[] { CreateParameter("sid", storeId) });

            // If deleted store was main branch, ensure at least one remaining store becomes main branch
            long mainCount = await ExecuteScalarAsync<long>("SELECT COUNT(*) FROM stores WHERE is_main_branch = 1");
            if (mainCount == 0)
            {
                await ExecuteNonQueryAsync(@"
                    UPDATE stores SET is_main_branch = 1 
                    WHERE store_id = (SELECT store_id FROM stores ORDER BY store_id ASC LIMIT 1)");
            }
        }

        public async Task ReassignAndDeleteAsync(int sourceStoreId, int targetStoreId)
        {
            var p = new[]
            {
                CreateParameter("sourceId", sourceStoreId),
                CreateParameter("targetId", targetStoreId)
            };

            await ExecuteNonQueryAsync("UPDATE customers SET store_id = @targetId WHERE store_id = @sourceId", p);
            await ExecuteNonQueryAsync("UPDATE service_requests SET store_id = @targetId WHERE store_id = @sourceId", p);
            await ExecuteNonQueryAsync("UPDATE users SET store_id = @targetId WHERE store_id = @sourceId", p);

            // If source was main branch, make target main branch
            var sourceStore = await GetByIdAsync(sourceStoreId);
            if (sourceStore != null && sourceStore.IsMainBranch)
            {
                await ExecuteNonQueryAsync("UPDATE stores SET is_main_branch = 1 WHERE store_id = @targetId", p);
            }

            await ExecuteNonQueryAsync("DELETE FROM stores WHERE store_id = @sourceId",
                new[] { CreateParameter("sourceId", sourceStoreId) });
        }

        public async Task SetMainBranchAsync(int storeId)
        {
            await ExecuteNonQueryAsync("UPDATE stores SET is_main_branch = 0");
            await ExecuteNonQueryAsync("UPDATE stores SET is_main_branch = 1 WHERE store_id = @sid",
                new[] { CreateParameter("sid", storeId) });
        }
    }
}
