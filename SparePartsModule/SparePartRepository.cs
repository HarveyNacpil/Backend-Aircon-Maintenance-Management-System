using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Api.Main;

namespace Api.SparePartsModule
{
    public class SparePartRepository : BaseRepository, ISparePartRepository
    {
        public SparePartRepository(MyCon dbConnection) : base(dbConnection) { }

        private const string PartCols = "part_id, part_name, category, quantity, unit_price, stock_status";

        private SparePart MapPart(DbDataReader r) => new SparePart
        {
            PartId      = ReadValue<int>(r,    "part_id",      0),
            PartName    = ReadValue<string>(r, "part_name",    string.Empty),
            Category    = ReadValue<string>(r, "category",     string.Empty),
            Quantity    = ReadValue<int>(r,    "quantity",     0),
            UnitPrice   = r.IsDBNull(r.GetOrdinal("unit_price")) ? null : r.GetDouble(r.GetOrdinal("unit_price")),
            StockStatus = ReadValue<string>(r, "stock_status", "In Stock"),
        };

        private PartUsage MapUsage(DbDataReader r) => new PartUsage
        {
            UsageId      = ReadValue<int>(r, "usage_id",      0),
            RecordId     = ReadValue<int>(r, "record_id",     0),
            PartId       = ReadValue<int>(r, "part_id",       0),
            QuantityUsed = ReadValue<int>(r, "quantity_used", 0),
        };

        public async Task<IEnumerable<SparePart>> GetAllPartsAsync()
        {
            string sql = $"SELECT {PartCols} FROM spare_parts ORDER BY part_name";
            return await ExecuteReaderToListAsync(sql, MapPart);
        }

        public async Task<SparePart?> GetPartByIdAsync(int partId)
        {
            string sql = $"SELECT {PartCols} FROM spare_parts WHERE part_id = @pid";
            var list = await ExecuteReaderToListAsync(sql, MapPart,
                new[] { CreateParameter("pid", partId) });
            return list.FirstOrDefault();
        }

        public async Task AddPartAsync(SparePart entity)
        {
            const string sql = @"
                INSERT INTO spare_parts (part_name, category, quantity, unit_price, stock_status)
                VALUES (@partName, @category, @quantity, @unitPrice, @stockStatus)";
            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("partName",    entity.PartName),
                CreateParameter("category",    (object?)entity.Category  ?? DBNull.Value),
                CreateParameter("quantity",    entity.Quantity),
                CreateParameter("unitPrice",   (object?)entity.UnitPrice ?? DBNull.Value),
                CreateParameter("stockStatus", entity.StockStatus),
            });
        }

        public async Task UpdatePartAsync(SparePart entity)
        {
            const string sql = @"
                UPDATE spare_parts SET
                    part_name    = @partName,
                    category     = @category,
                    quantity     = @quantity,
                    unit_price   = @unitPrice,
                    stock_status = @stockStatus
                WHERE part_id = @partId";
            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("partId",      entity.PartId),
                CreateParameter("partName",    entity.PartName),
                CreateParameter("category",    (object?)entity.Category  ?? DBNull.Value),
                CreateParameter("quantity",    entity.Quantity),
                CreateParameter("unitPrice",   (object?)entity.UnitPrice ?? DBNull.Value),
                CreateParameter("stockStatus", entity.StockStatus),
            });
        }

        public async Task DeletePartAsync(int partId)
        {
            const string sql = "DELETE FROM spare_parts WHERE part_id = @pid";
            await ExecuteNonQueryAsync(sql, new[] { CreateParameter("pid", partId) });
        }

        public async Task<IEnumerable<SparePart>> SearchPartsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return await GetAllPartsAsync();
            string sql = $@"
                SELECT {PartCols} FROM spare_parts
                WHERE part_name LIKE @q OR category LIKE @q OR stock_status LIKE @q";
            return await ExecuteReaderToListAsync(sql, MapPart,
                new[] { CreateParameter("q", "%" + query + "%") });
        }

        public async Task<PaginationModel<SparePart>> GetPartsPaginatedAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize   <= 0) pageSize   = 10;

            string sql = $@"
                SELECT {PartCols} FROM spare_parts
                ORDER BY part_name LIMIT @size OFFSET @offset";

            var items = await ExecuteReaderToListAsync(sql, MapPart, new[]
            {
                CreateParameter("size",   pageSize),
                CreateParameter("offset", (pageNumber - 1) * pageSize),
            });
            int total = (await ExecuteScalarAsync<int?>("SELECT COUNT(*) FROM spare_parts")) ?? 0;
            return new PaginationModel<SparePart>
            {
                Items = items, TotalCount = total, PageSize = pageSize, CurrentPage = pageNumber,
            };
        }

        // Parts Usage
        public async Task<IEnumerable<PartUsage>> GetUsageByRecordAsync(int recordId)
        {
            const string sql = "SELECT usage_id, record_id, part_id, quantity_used FROM parts_usage WHERE record_id = @rid";
            return await ExecuteReaderToListAsync(sql, MapUsage,
                new[] { CreateParameter("rid", recordId) });
        }

        public async Task AddUsageAsync(PartUsage entity)
        {
            const string sql = @"
                INSERT INTO parts_usage (record_id, part_id, quantity_used)
                VALUES (@recordId, @partId, @quantityUsed)";
            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("recordId",     entity.RecordId),
                CreateParameter("partId",       entity.PartId),
                CreateParameter("quantityUsed", entity.QuantityUsed),
            });
        }

        public async Task DeleteUsageAsync(int usageId)
        {
            const string sql = "DELETE FROM parts_usage WHERE usage_id = @uid";
            await ExecuteNonQueryAsync(sql, new[] { CreateParameter("uid", usageId) });
        }
    }
}
