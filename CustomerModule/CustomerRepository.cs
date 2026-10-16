using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Api.Main;

namespace Api.CustomerModule
{
    public class CustomerRepository : BaseRepository, ICustomerRepository
    {
        public CustomerRepository(MyCon dbConnection) : base(dbConnection) { }

        private const string SelectCols =
            "customer_id, first_name, last_name, phone, email, address, region_code, region_name, province_code, province_name, city_code, city_name, barangay_code, barangay_name, street_address, latitude, longitude, store_id, created_at";

        private Customer MapReader(DbDataReader r) => new Customer
        {
            CustomerId    = ReadValue<int>(r,     "customer_id", 0),
            FirstName     = ReadValue<string>(r,  "first_name",  string.Empty),
            LastName      = ReadValue<string>(r,  "last_name",   string.Empty),
            Phone         = ReadValue<string?>(r, "phone",       null),
            Email         = ReadValue<string?>(r, "email",       null),
            Address       = ReadValue<string?>(r, "address",     null),
            RegionCode    = ReadValue<string?>(r, "region_code", null),
            RegionName    = ReadValue<string?>(r, "region_name", null),
            ProvinceCode  = ReadValue<string?>(r, "province_code", null),
            ProvinceName  = ReadValue<string?>(r, "province_name", null),
            CityCode      = ReadValue<string?>(r, "city_code", null),
            CityName      = ReadValue<string?>(r, "city_name", null),
            BarangayCode  = ReadValue<string?>(r, "barangay_code", null),
            BarangayName  = ReadValue<string?>(r, "barangay_name", null),
            StreetAddress = ReadValue<string?>(r, "street_address", null),
            Latitude      = ReadValue<double?>(r, "latitude", null),
            Longitude     = ReadValue<double?>(r, "longitude", null),
            StoreId       = ReadValue<int?>(r,    "store_id", null),
            CreatedAt     = ReadValue<string?>(r, "created_at", null),
        };

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            string sql = $"SELECT {SelectCols} FROM customers ORDER BY last_name, first_name";
            return await ExecuteReaderToListAsync(sql, MapReader);
        }

        public async Task<Customer?> GetByIdAsync(int customerId)
        {
            string sql = $"SELECT {SelectCols} FROM customers WHERE customer_id = @cid";
            var list = await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("cid", customerId) });
            return list.FirstOrDefault();
        }

        public async Task AddAsync(Customer entity)
        {
            const string sql = @"
                INSERT INTO customers (
                    first_name, last_name, phone, email, address,
                    region_code, region_name, province_code, province_name,
                    city_code, city_name, barangay_code, barangay_name, street_address,
                    latitude, longitude, store_id
                )
                VALUES (
                    @firstName, @lastName, @phone, @email, @address,
                    @regionCode, @regionName, @provinceCode, @provinceName,
                    @cityCode, @cityName, @barangayCode, @barangayName, @streetAddress,
                    @latitude, @longitude, @storeId
                )";
            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("firstName",     entity.FirstName),
                CreateParameter("lastName",      entity.LastName),
                CreateParameter("phone",         (object?)entity.Phone         ?? DBNull.Value),
                CreateParameter("email",         (object?)entity.Email         ?? DBNull.Value),
                CreateParameter("address",       (object?)entity.Address       ?? DBNull.Value),
                CreateParameter("regionCode",    (object?)entity.RegionCode    ?? DBNull.Value),
                CreateParameter("regionName",    (object?)entity.RegionName    ?? DBNull.Value),
                CreateParameter("provinceCode",  (object?)entity.ProvinceCode  ?? DBNull.Value),
                CreateParameter("provinceName",  (object?)entity.ProvinceName  ?? DBNull.Value),
                CreateParameter("cityCode",      (object?)entity.CityCode      ?? DBNull.Value),
                CreateParameter("cityName",      (object?)entity.CityName      ?? DBNull.Value),
                CreateParameter("barangayCode",  (object?)entity.BarangayCode  ?? DBNull.Value),
                CreateParameter("barangayName",  (object?)entity.BarangayName  ?? DBNull.Value),
                CreateParameter("streetAddress", (object?)entity.StreetAddress ?? DBNull.Value),
                CreateParameter("latitude",      (object?)entity.Latitude      ?? DBNull.Value),
                CreateParameter("longitude",     (object?)entity.Longitude     ?? DBNull.Value),
                CreateParameter("storeId",       (object?)entity.StoreId       ?? DBNull.Value),
            });
        }

        public async Task UpdateAsync(Customer entity)
        {
            const string sql = @"
                UPDATE customers SET
                    first_name     = @firstName,
                    last_name      = @lastName,
                    phone          = @phone,
                    email          = @email,
                    address        = @address,
                    region_code    = @regionCode,
                    region_name    = @regionName,
                    province_code  = @provinceCode,
                    province_name  = @provinceName,
                    city_code      = @cityCode,
                    city_name      = @cityName,
                    barangay_code  = @barangayCode,
                    barangay_name  = @barangayName,
                    street_address = @streetAddress,
                    latitude       = @latitude,
                    longitude      = @longitude,
                    store_id       = @storeId
                WHERE customer_id = @cid";
            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("cid",           entity.CustomerId),
                CreateParameter("firstName",     entity.FirstName),
                CreateParameter("lastName",      entity.LastName),
                CreateParameter("phone",         (object?)entity.Phone         ?? DBNull.Value),
                CreateParameter("email",         (object?)entity.Email         ?? DBNull.Value),
                CreateParameter("address",       (object?)entity.Address       ?? DBNull.Value),
                CreateParameter("regionCode",    (object?)entity.RegionCode    ?? DBNull.Value),
                CreateParameter("regionName",    (object?)entity.RegionName    ?? DBNull.Value),
                CreateParameter("provinceCode",  (object?)entity.ProvinceCode  ?? DBNull.Value),
                CreateParameter("provinceName",  (object?)entity.ProvinceName  ?? DBNull.Value),
                CreateParameter("cityCode",      (object?)entity.CityCode      ?? DBNull.Value),
                CreateParameter("cityName",      (object?)entity.CityName      ?? DBNull.Value),
                CreateParameter("barangayCode",  (object?)entity.BarangayCode  ?? DBNull.Value),
                CreateParameter("barangayName",  (object?)entity.BarangayName  ?? DBNull.Value),
                CreateParameter("streetAddress", (object?)entity.StreetAddress ?? DBNull.Value),
                CreateParameter("latitude",      (object?)entity.Latitude      ?? DBNull.Value),
                CreateParameter("longitude",     (object?)entity.Longitude     ?? DBNull.Value),
                CreateParameter("storeId",       (object?)entity.StoreId       ?? DBNull.Value),
            });
        }

        public async Task DeleteAsync(int customerId)
        {
            const string sql = "DELETE FROM customers WHERE customer_id = @cid";
            await ExecuteNonQueryAsync(sql, new[] { CreateParameter("cid", customerId) });
        }

        public async Task<IEnumerable<Customer>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return await GetAllAsync();
            string sql = $@"
                SELECT {SelectCols} FROM customers
                WHERE first_name LIKE @q OR last_name LIKE @q OR phone LIKE @q OR email LIKE @q OR address LIKE @q";
            return await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("q", "%" + query + "%") });
        }

        public async Task<PaginationModel<Customer>> GetPaginatedAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize   <= 0) pageSize   = 10;

            string sql = $@"
                SELECT {SelectCols} FROM customers
                ORDER BY last_name, first_name LIMIT @size OFFSET @offset";

            var items = await ExecuteReaderToListAsync(sql, MapReader, new[]
            {
                CreateParameter("size",   pageSize),
                CreateParameter("offset", (pageNumber - 1) * pageSize),
            });
            int total = (await ExecuteScalarAsync<int?>("SELECT COUNT(*) FROM customers")) ?? 0;
            return new PaginationModel<Customer>
            {
                Items       = items,
                TotalCount  = total,
                PageSize    = pageSize,
                CurrentPage = pageNumber,
            };
        }

        public async Task<IEnumerable<CustomerMapItem>> GetCustomerMapLocationsAsync()
        {
            // Only include customers with valid coordinates and AT LEAST 1 aircon unit
            const string sql = @"
                SELECT c.customer_id, c.first_name, c.last_name, c.phone, c.email, c.address,
                       c.region_code, c.region_name, c.province_code, c.province_name,
                       c.city_code, c.city_name, c.barangay_code, c.barangay_name,
                       c.street_address, c.latitude, c.longitude,
                       COUNT(u.unit_id) as aircon_count
                FROM customers c
                INNER JOIN aircon_units u ON c.customer_id = u.customer_id
                WHERE c.latitude IS NOT NULL AND c.longitude IS NOT NULL
                GROUP BY c.customer_id
                HAVING COUNT(u.unit_id) > 0
                ORDER BY c.last_name, c.first_name";

            var mapItems = (await ExecuteReaderToListAsync(sql, r => new CustomerMapItem
            {
                CustomerId    = ReadValue<int>(r,     "customer_id", 0),
                FirstName     = ReadValue<string>(r,  "first_name",  string.Empty),
                LastName      = ReadValue<string>(r,  "last_name",   string.Empty),
                Phone         = ReadValue<string?>(r, "phone",       null),
                Email         = ReadValue<string?>(r, "email",       null),
                Address       = ReadValue<string?>(r, "address",     null),
                RegionCode    = ReadValue<string?>(r, "region_code", null),
                RegionName    = ReadValue<string?>(r, "region_name", null),
                ProvinceCode  = ReadValue<string?>(r, "province_code", null),
                ProvinceName  = ReadValue<string?>(r, "province_name", null),
                CityCode      = ReadValue<string?>(r, "city_code", null),
                CityName      = ReadValue<string?>(r, "city_name", null),
                BarangayCode  = ReadValue<string?>(r, "barangay_code", null),
                BarangayName  = ReadValue<string?>(r, "barangay_name", null),
                StreetAddress = ReadValue<string?>(r, "street_address", null),
                Latitude      = ReadValue<double>(r,  "latitude",    0),
                Longitude     = ReadValue<double>(r,  "longitude",   0),
                AirconCount   = ReadValue<int>(r,     "aircon_count", 0),
            })).ToList();

            if (!mapItems.Any()) return mapItems;

            var custIds = string.Join(",", mapItems.Select(m => m.CustomerId));

            // Fetch aircon units for these customers
            string unitsSql = $@"
                SELECT unit_id, customer_id, brand, model, horsepower, serial_number, location, status,
                       installation_date, warranty_expiry
                FROM aircon_units
                WHERE customer_id IN ({custIds})
                ORDER BY brand, model";

            var units = await ExecuteReaderToListAsync(unitsSql, r => new
            {
                CustomerId       = ReadValue<int>(r, "customer_id", 0),
                UnitId           = ReadValue<int>(r, "unit_id", 0),
                Brand            = ReadValue<string>(r, "brand", string.Empty),
                Model            = ReadValue<string>(r, "model", string.Empty),
                Horsepower       = ReadValue<double>(r, "horsepower", 0),
                SerialNumber     = ReadValue<string?>(r, "serial_number", null),
                Location         = ReadValue<string?>(r, "location", null),
                Status           = ReadValue<string>(r, "status", "Active"),
                InstallationDate = ReadValue<string?>(r, "installation_date", null),
                WarrantyExpiry   = ReadValue<string?>(r, "warranty_expiry", null),
            });

            // Fetch upcoming maintenance schedules
            string schedSql = $@"
                SELECT schedule_id, customer_id, unit_id, schedule_date, schedule_time, status, remarks
                FROM maintenance_schedule
                WHERE customer_id IN ({custIds}) AND status != 'Cancelled'
                ORDER BY schedule_date ASC";

            var schedules = await ExecuteReaderToListAsync(schedSql, r => new
            {
                CustomerId   = ReadValue<int>(r, "customer_id", 0),
                ScheduleId   = ReadValue<int>(r, "schedule_id", 0),
                UnitId       = ReadValue<int>(r, "unit_id", 0),
                ScheduleDate = ReadValue<string>(r, "schedule_date", string.Empty),
                ScheduleTime = ReadValue<string?>(r, "schedule_time", null),
                Status       = ReadValue<string>(r, "status", "Scheduled"),
                Remarks      = ReadValue<string?>(r, "remarks", null),
            });

            var unitsGrouped = units.GroupBy(u => u.CustomerId).ToDictionary(g => g.Key, g => g.ToList());
            var schedGrouped = schedules.GroupBy(s => s.CustomerId).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var item in mapItems)
            {
                if (unitsGrouped.TryGetValue(item.CustomerId, out var uList))
                {
                    item.AirconUnits = uList.Select(u => new CustomerAirconSummary
                    {
                        UnitId           = u.UnitId,
                        Brand            = u.Brand,
                        Model            = u.Model,
                        Horsepower       = u.Horsepower,
                        SerialNumber     = u.SerialNumber,
                        Location         = u.Location,
                        Status           = u.Status,
                        InstallationDate = u.InstallationDate,
                        WarrantyExpiry   = u.WarrantyExpiry,
                    }).ToList();
                }

                if (schedGrouped.TryGetValue(item.CustomerId, out var sList))
                {
                    var next = sList.FirstOrDefault(s => s.Status == "Scheduled" || s.Status == "In Progress") ?? sList.FirstOrDefault();
                    if (next != null)
                    {
                        item.UpcomingMaintenance = new CustomerMaintenanceSummary
                        {
                            ScheduleId   = next.ScheduleId,
                            UnitId       = next.UnitId,
                            ScheduleDate = next.ScheduleDate,
                            ScheduleTime = next.ScheduleTime,
                            Status       = next.Status,
                            Remarks      = next.Remarks,
                        };
                    }
                }
            }

            return mapItems;
        }

        public async Task UpdateCoordinatesAsync(int customerId, double latitude, double longitude)
        {
            const string sql = "UPDATE customers SET latitude = @lat, longitude = @lng WHERE customer_id = @cid";
            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("lat", latitude),
                CreateParameter("lng", longitude),
                CreateParameter("cid", customerId),
            });
        }
    }
}
