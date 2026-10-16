using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Api.Main;

namespace Api.Airconmodule
{
    public class AirconRepository : BaseRepository, IAirconRepository
    {
        public AirconRepository(MyCon dbConnection) : base(dbConnection) { }

        private const string SelectCols = @"
            unit_id, customer_id, brand, model, horsepower,
            serial_number, installation_date, warranty_expiry, location, status";

        private AirconUnit MapReader(DbDataReader r) => new AirconUnit
        {
            UnitId           = ReadValue<int>(r,    "unit_id",           0),
            CustomerId       = ReadValue<int>(r,    "customer_id",       0),
            Brand            = ReadValue<string>(r, "brand",             string.Empty),
            Model            = ReadValue<string>(r, "model",             string.Empty),
            Horsepower       = r.IsDBNull(r.GetOrdinal("horsepower")) ? null : r.GetDouble(r.GetOrdinal("horsepower")),
            SerialNumber     = ReadValue<string>(r, "serial_number",     string.Empty),
            InstallationDate = ReadValue<string>(r, "installation_date", string.Empty),
            WarrantyExpiry   = ReadValue<string>(r, "warranty_expiry",   string.Empty),
            Location         = ReadValue<string>(r, "location",          string.Empty),
            Status           = ReadValue<string>(r, "status",            "Active"),
        };

        public async Task<IEnumerable<AirconUnit>> GetAllAsync()
        {
            string sql = $"SELECT {SelectCols} FROM aircon_units ORDER BY unit_id";
            return await ExecuteReaderToListAsync(sql, MapReader);
        }

        public async Task<IEnumerable<AirconUnit>> GetByCustomerIdAsync(int customerId)
        {
            string sql = $"SELECT {SelectCols} FROM aircon_units WHERE customer_id = @cid ORDER BY unit_id";
            return await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("cid", customerId) });
        }

        public async Task<AirconUnit?> GetByIdAsync(int unitId)
        {
            string sql = $"SELECT {SelectCols} FROM aircon_units WHERE unit_id = @uid";
            var list = await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("uid", unitId) });
            return list.FirstOrDefault();
        }

        public async Task AddAsync(AirconUnit entity)
        {
            const string sql = @"
                INSERT INTO aircon_units
                    (customer_id, brand, model, horsepower, serial_number,
                     installation_date, warranty_expiry, location, status)
                VALUES
                    (@customerId, @brand, @model, @horsepower, @serialNumber,
                     @installationDate, @warrantyExpiry, @location, @status)";
            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("customerId",       entity.CustomerId),
                CreateParameter("brand",            (object?)entity.Brand            ?? DBNull.Value),
                CreateParameter("model",            (object?)entity.Model            ?? DBNull.Value),
                CreateParameter("horsepower",       (object?)entity.Horsepower       ?? DBNull.Value),
                CreateParameter("serialNumber",     (object?)entity.SerialNumber     ?? DBNull.Value),
                CreateParameter("installationDate", (object?)entity.InstallationDate ?? DBNull.Value),
                CreateParameter("warrantyExpiry",   (object?)entity.WarrantyExpiry   ?? DBNull.Value),
                CreateParameter("location",         (object?)entity.Location         ?? DBNull.Value),
                CreateParameter("status",           entity.Status),
            });
        }

        public async Task UpdateAsync(AirconUnit entity)
        {
            const string sql = @"
                UPDATE aircon_units SET
                    customer_id       = @customerId,
                    brand             = @brand,
                    model             = @model,
                    horsepower        = @horsepower,
                    serial_number     = @serialNumber,
                    installation_date = @installationDate,
                    warranty_expiry   = @warrantyExpiry,
                    location          = @location,
                    status            = @status
                WHERE unit_id = @unitId";
            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("unitId",           entity.UnitId),
                CreateParameter("customerId",       entity.CustomerId),
                CreateParameter("brand",            (object?)entity.Brand            ?? DBNull.Value),
                CreateParameter("model",            (object?)entity.Model            ?? DBNull.Value),
                CreateParameter("horsepower",       (object?)entity.Horsepower       ?? DBNull.Value),
                CreateParameter("serialNumber",     (object?)entity.SerialNumber     ?? DBNull.Value),
                CreateParameter("installationDate", (object?)entity.InstallationDate ?? DBNull.Value),
                CreateParameter("warrantyExpiry",   (object?)entity.WarrantyExpiry   ?? DBNull.Value),
                CreateParameter("location",         (object?)entity.Location         ?? DBNull.Value),
                CreateParameter("status",           entity.Status),
            });
        }

        public async Task DeleteAsync(int unitId)
        {
            const string sql = "DELETE FROM aircon_units WHERE unit_id = @uid";
            await ExecuteNonQueryAsync(sql, new[] { CreateParameter("uid", unitId) });
        }

        public async Task<IEnumerable<AirconUnit>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return await GetAllAsync();
            string sql = $@"
                SELECT {SelectCols} FROM aircon_units
                WHERE brand LIKE @q OR model LIKE @q OR serial_number LIKE @q OR location LIKE @q OR status LIKE @q";
            return await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("q", "%" + query + "%") });
        }

        public async Task<PaginationModel<AirconUnit>> GetPaginatedAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize   <= 0) pageSize   = 10;

            string sql = $@"
                SELECT {SelectCols} FROM aircon_units
                ORDER BY unit_id LIMIT @size OFFSET @offset";

            var items = await ExecuteReaderToListAsync(sql, MapReader, new[]
            {
                CreateParameter("size",   pageSize),
                CreateParameter("offset", (pageNumber - 1) * pageSize),
            });
            int total = (await ExecuteScalarAsync<int?>("SELECT COUNT(*) FROM aircon_units")) ?? 0;
            return new PaginationModel<AirconUnit>
            {
                Items       = items,
                TotalCount  = total,
                PageSize    = pageSize,
                CurrentPage = pageNumber,
            };
        }
    }
}

