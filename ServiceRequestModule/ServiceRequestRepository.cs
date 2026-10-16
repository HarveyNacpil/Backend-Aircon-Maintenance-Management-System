using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Api.Main;

namespace Api.ServiceRequestModule
{
    public class ServiceRequestRepository : BaseRepository, IServiceRequestRepository
    {
        public ServiceRequestRepository(MyCon dbConnection) : base(dbConnection) { }

        private const string SelectCols =
            "request_id, customer_id, unit_id, issue_description, priority, status, assigned_technician, request_date";

        private ServiceRequest MapReader(DbDataReader r) => new ServiceRequest
        {
            RequestId           = ReadValue<int>(r,    "request_id",          0),
            CustomerId          = ReadValue<int>(r,    "customer_id",          0),
            UnitId              = ReadValue<int>(r,    "unit_id",              0),
            IssueDescription    = ReadValue<string>(r, "issue_description",    string.Empty),
            Priority            = ReadValue<string>(r, "priority",             "Medium"),
            Status              = ReadValue<string>(r, "status",               "Open"),
            AssignedTechnician  = r.IsDBNull(r.GetOrdinal("assigned_technician")) ? null : r.GetInt32(r.GetOrdinal("assigned_technician")),
            RequestDate         = ReadValue<string>(r, "request_date",         string.Empty),
        };

        public async Task<IEnumerable<ServiceRequest>> GetAllAsync()
        {
            string sql = $"SELECT {SelectCols} FROM service_requests ORDER BY request_id DESC";
            return await ExecuteReaderToListAsync(sql, MapReader);
        }

        public async Task<ServiceRequest?> GetByIdAsync(int requestId)
        {
            string sql = $"SELECT {SelectCols} FROM service_requests WHERE request_id = @rid";
            var list = await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("rid", requestId) });
            return list.FirstOrDefault();
        }

        public async Task<IEnumerable<ServiceRequest>> GetByCustomerAsync(int customerId)
        {
            string sql = $"SELECT {SelectCols} FROM service_requests WHERE customer_id = @cid ORDER BY request_id DESC";
            return await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("cid", customerId) });
        }

        public async Task<IEnumerable<ServiceRequest>> GetByTechnicianAsync(int technicianId)
        {
            string sql = $"SELECT {SelectCols} FROM service_requests WHERE assigned_technician = @tid ORDER BY request_id DESC";
            return await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("tid", technicianId) });
        }

        public async Task<IEnumerable<ServiceRequest>> GetByStatusAsync(string status)
        {
            string sql = $"SELECT {SelectCols} FROM service_requests WHERE status = @status ORDER BY request_id DESC";
            return await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("status", status) });
        }

        public async Task AddAsync(ServiceRequest entity)
        {
            const string sql = @"
                INSERT INTO service_requests
                    (customer_id, unit_id, issue_description, priority, status, assigned_technician)
                VALUES
                    (@customerId, @unitId, @issueDescription, @priority, @status, @assignedTechnician)";
            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("customerId",         entity.CustomerId),
                CreateParameter("unitId",             entity.UnitId),
                CreateParameter("issueDescription",   entity.IssueDescription),
                CreateParameter("priority",           entity.Priority),
                CreateParameter("status",             entity.Status),
                CreateParameter("assignedTechnician", (object?)entity.AssignedTechnician ?? DBNull.Value),
            });
        }

        public async Task UpdateAsync(ServiceRequest entity)
        {
            const string sql = @"
                UPDATE service_requests SET
                    customer_id          = @customerId,
                    unit_id              = @unitId,
                    issue_description    = @issueDescription,
                    priority             = @priority,
                    status               = @status,
                    assigned_technician  = @assignedTechnician
                WHERE request_id = @requestId";
            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("requestId",          entity.RequestId),
                CreateParameter("customerId",         entity.CustomerId),
                CreateParameter("unitId",             entity.UnitId),
                CreateParameter("issueDescription",   entity.IssueDescription),
                CreateParameter("priority",           entity.Priority),
                CreateParameter("status",             entity.Status),
                CreateParameter("assignedTechnician", (object?)entity.AssignedTechnician ?? DBNull.Value),
            });
        }

        public async Task AssignTechnicianAsync(int requestId, int technicianId)
        {
            const string sql = @"
                UPDATE service_requests
                SET assigned_technician = @techId, status = 'Assigned'
                WHERE request_id = @rid";
            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("rid",    requestId),
                CreateParameter("techId", technicianId),
            });
        }

        public async Task UpdateStatusAsync(int requestId, string status)
        {
            const string sql = "UPDATE service_requests SET status = @status WHERE request_id = @rid";
            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("rid",    requestId),
                CreateParameter("status", status),
            });
        }

        public async Task DeleteAsync(int requestId)
        {
            const string sql = "DELETE FROM service_requests WHERE request_id = @rid";
            await ExecuteNonQueryAsync(sql, new[] { CreateParameter("rid", requestId) });
        }

        public async Task<PaginationModel<ServiceRequest>> GetPaginatedAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize   <= 0) pageSize   = 10;

            string sql = $@"
                SELECT {SelectCols} FROM service_requests
                ORDER BY request_id DESC LIMIT @size OFFSET @offset";

            var items = await ExecuteReaderToListAsync(sql, MapReader, new[]
            {
                CreateParameter("size",   pageSize),
                CreateParameter("offset", (pageNumber - 1) * pageSize),
            });
            int total = (await ExecuteScalarAsync<int?>("SELECT COUNT(*) FROM service_requests")) ?? 0;
            return new PaginationModel<ServiceRequest>
            {
                Items = items, TotalCount = total, PageSize = pageSize, CurrentPage = pageNumber,
            };
        }
    }
}
