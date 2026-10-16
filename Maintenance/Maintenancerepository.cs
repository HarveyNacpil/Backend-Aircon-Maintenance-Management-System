using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Api.Main;

namespace Api.Maintenance
{
    public class MaintenanceRepository : BaseRepository, IMaintenanceRepository
    {
        public MaintenanceRepository(MyCon dbConnection) : base(dbConnection) { }

        // =====================================================================
        // SCHEDULE
        // =====================================================================
        private const string SchedCols =
            "schedule_id, customer_id, unit_id, technician_id, schedule_date, schedule_time, status, remarks";

        private MaintenanceSchedule MapSched(DbDataReader r) => new MaintenanceSchedule
        {
            ScheduleId   = ReadValue<int>(r,    "schedule_id",   0),
            CustomerId   = ReadValue<int>(r,    "customer_id",   0),
            UnitId       = ReadValue<int>(r,    "unit_id",       0),
            TechnicianId = ReadValue<int>(r,    "technician_id", 0),
            ScheduleDate = ReadValue<string>(r, "schedule_date", string.Empty),
            ScheduleTime = ReadValue<string>(r, "schedule_time", string.Empty),
            Status       = ReadValue<string>(r, "status",        "Scheduled"),
            Remarks      = ReadValue<string>(r, "remarks",       string.Empty),
        };

        public async Task<IEnumerable<MaintenanceSchedule>> GetAllSchedulesAsync()
        {
            string sql = $"SELECT {SchedCols} FROM maintenance_schedule ORDER BY schedule_date, schedule_time";
            return await ExecuteReaderToListAsync(sql, MapSched);
        }

        public async Task<MaintenanceSchedule?> GetScheduleByIdAsync(int scheduleId)
        {
            string sql = $"SELECT {SchedCols} FROM maintenance_schedule WHERE schedule_id = @sid";
            var list = await ExecuteReaderToListAsync(sql, MapSched,
                new[] { CreateParameter("sid", scheduleId) });
            return list.FirstOrDefault();
        }

        public async Task<IEnumerable<MaintenanceSchedule>> GetSchedulesByCustomerAsync(int customerId)
        {
            string sql = $"SELECT {SchedCols} FROM maintenance_schedule WHERE customer_id = @cid ORDER BY schedule_date";
            return await ExecuteReaderToListAsync(sql, MapSched,
                new[] { CreateParameter("cid", customerId) });
        }

        public async Task<IEnumerable<MaintenanceSchedule>> GetSchedulesByTechnicianAsync(int technicianId)
        {
            string sql = $"SELECT {SchedCols} FROM maintenance_schedule WHERE technician_id = @tid ORDER BY schedule_date";
            return await ExecuteReaderToListAsync(sql, MapSched,
                new[] { CreateParameter("tid", technicianId) });
        }

        public async Task AddScheduleAsync(MaintenanceSchedule entity)
        {
            const string sql = @"
                INSERT INTO maintenance_schedule
                    (customer_id, unit_id, technician_id, schedule_date, schedule_time, status, remarks)
                VALUES
                    (@customerId, @unitId, @technicianId, @scheduleDate, @scheduleTime, @status, @remarks)";
            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("customerId",   entity.CustomerId),
                CreateParameter("unitId",       entity.UnitId),
                CreateParameter("technicianId", entity.TechnicianId),
                CreateParameter("scheduleDate", entity.ScheduleDate),
                CreateParameter("scheduleTime", entity.ScheduleTime),
                CreateParameter("status",       entity.Status),
                CreateParameter("remarks",      (object?)entity.Remarks ?? DBNull.Value),
            });
        }

        public async Task UpdateScheduleAsync(MaintenanceSchedule entity)
        {
            const string sql = @"
                UPDATE maintenance_schedule SET
                    customer_id   = @customerId,
                    unit_id       = @unitId,
                    technician_id = @technicianId,
                    schedule_date = @scheduleDate,
                    schedule_time = @scheduleTime,
                    status        = @status,
                    remarks       = @remarks
                WHERE schedule_id = @scheduleId";
            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("scheduleId",   entity.ScheduleId),
                CreateParameter("customerId",   entity.CustomerId),
                CreateParameter("unitId",       entity.UnitId),
                CreateParameter("technicianId", entity.TechnicianId),
                CreateParameter("scheduleDate", entity.ScheduleDate),
                CreateParameter("scheduleTime", entity.ScheduleTime),
                CreateParameter("status",       entity.Status),
                CreateParameter("remarks",      (object?)entity.Remarks ?? DBNull.Value),
            });
        }

        public async Task DeleteScheduleAsync(int scheduleId)
        {
            const string sql = "DELETE FROM maintenance_schedule WHERE schedule_id = @sid";
            await ExecuteNonQueryAsync(sql, new[] { CreateParameter("sid", scheduleId) });
        }

        public async Task<PaginationModel<MaintenanceSchedule>> GetSchedulesPaginatedAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize   <= 0) pageSize   = 10;

            string sql = $@"
                SELECT {SchedCols} FROM maintenance_schedule
                ORDER BY schedule_date, schedule_time LIMIT @size OFFSET @offset";

            var items = await ExecuteReaderToListAsync(sql, MapSched, new[]
            {
                CreateParameter("size",   pageSize),
                CreateParameter("offset", (pageNumber - 1) * pageSize),
            });
            int total = (await ExecuteScalarAsync<int?>("SELECT COUNT(*) FROM maintenance_schedule")) ?? 0;
            return new PaginationModel<MaintenanceSchedule>
            {
                Items = items, TotalCount = total, PageSize = pageSize, CurrentPage = pageNumber,
            };
        }

        // =====================================================================
        // RECORDS
        // =====================================================================
        private const string RecordCols =
            "record_id, schedule_id, work_performed, parts_replaced, refrigerant_added, technician_notes, next_service_date, completed_date";

        private MaintenanceRecord MapRecord(DbDataReader r) => new MaintenanceRecord
        {
            RecordId        = ReadValue<int>(r,    "record_id",        0),
            ScheduleId      = ReadValue<int>(r,    "schedule_id",      0),
            WorkPerformed   = ReadValue<string>(r, "work_performed",   string.Empty),
            PartsReplaced   = ReadValue<string>(r, "parts_replaced",   string.Empty),
            RefrigerantAdded= ReadValue<string>(r, "refrigerant_added",string.Empty),
            TechnicianNotes = ReadValue<string>(r, "technician_notes", string.Empty),
            NextServiceDate = ReadValue<string>(r, "next_service_date",string.Empty),
            CompletedDate   = ReadValue<string>(r, "completed_date",   string.Empty),
        };

        public async Task<IEnumerable<MaintenanceRecord>> GetAllRecordsAsync()
        {
            string sql = $"SELECT {RecordCols} FROM maintenance_records ORDER BY record_id";
            return await ExecuteReaderToListAsync(sql, MapRecord);
        }

        public async Task<MaintenanceRecord?> GetRecordByIdAsync(int recordId)
        {
            string sql = $"SELECT {RecordCols} FROM maintenance_records WHERE record_id = @rid";
            var list = await ExecuteReaderToListAsync(sql, MapRecord,
                new[] { CreateParameter("rid", recordId) });
            return list.FirstOrDefault();
        }

        public async Task<MaintenanceRecord?> GetRecordByScheduleIdAsync(int scheduleId)
        {
            string sql = $"SELECT {RecordCols} FROM maintenance_records WHERE schedule_id = @sid";
            var list = await ExecuteReaderToListAsync(sql, MapRecord,
                new[] { CreateParameter("sid", scheduleId) });
            return list.FirstOrDefault();
        }

        public async Task AddRecordAsync(MaintenanceRecord entity)
        {
            const string sql = @"
                INSERT INTO maintenance_records
                    (schedule_id, work_performed, parts_replaced, refrigerant_added,
                     technician_notes, next_service_date, completed_date)
                VALUES
                    (@scheduleId, @workPerformed, @partsReplaced, @refrigerantAdded,
                     @technicianNotes, @nextServiceDate, @completedDate)";
            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("scheduleId",      entity.ScheduleId),
                CreateParameter("workPerformed",   (object?)entity.WorkPerformed    ?? DBNull.Value),
                CreateParameter("partsReplaced",   (object?)entity.PartsReplaced    ?? DBNull.Value),
                CreateParameter("refrigerantAdded",(object?)entity.RefrigerantAdded ?? DBNull.Value),
                CreateParameter("technicianNotes", (object?)entity.TechnicianNotes  ?? DBNull.Value),
                CreateParameter("nextServiceDate", (object?)entity.NextServiceDate  ?? DBNull.Value),
                CreateParameter("completedDate",   (object?)entity.CompletedDate    ?? DBNull.Value),
            });
        }

        public async Task UpdateRecordAsync(MaintenanceRecord entity)
        {
            const string sql = @"
                UPDATE maintenance_records SET
                    work_performed    = @workPerformed,
                    parts_replaced    = @partsReplaced,
                    refrigerant_added = @refrigerantAdded,
                    technician_notes  = @technicianNotes,
                    next_service_date = @nextServiceDate,
                    completed_date    = @completedDate
                WHERE record_id = @recordId";
            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("recordId",        entity.RecordId),
                CreateParameter("workPerformed",   (object?)entity.WorkPerformed    ?? DBNull.Value),
                CreateParameter("partsReplaced",   (object?)entity.PartsReplaced    ?? DBNull.Value),
                CreateParameter("refrigerantAdded",(object?)entity.RefrigerantAdded ?? DBNull.Value),
                CreateParameter("technicianNotes", (object?)entity.TechnicianNotes  ?? DBNull.Value),
                CreateParameter("nextServiceDate", (object?)entity.NextServiceDate  ?? DBNull.Value),
                CreateParameter("completedDate",   (object?)entity.CompletedDate    ?? DBNull.Value),
            });
        }

        public async Task DeleteRecordAsync(int recordId)
        {
            const string sql = "DELETE FROM maintenance_records WHERE record_id = @rid";
            await ExecuteNonQueryAsync(sql, new[] { CreateParameter("rid", recordId) });
        }

        public async Task<PaginationModel<MaintenanceRecord>> GetRecordsPaginatedAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize   <= 0) pageSize   = 10;

            string sql = $@"
                SELECT {RecordCols} FROM maintenance_records
                ORDER BY record_id LIMIT @size OFFSET @offset";

            var items = await ExecuteReaderToListAsync(sql, MapRecord, new[]
            {
                CreateParameter("size",   pageSize),
                CreateParameter("offset", (pageNumber - 1) * pageSize),
            });
            int total = (await ExecuteScalarAsync<int?>("SELECT COUNT(*) FROM maintenance_records")) ?? 0;
            return new PaginationModel<MaintenanceRecord>
            {
                Items = items, TotalCount = total, PageSize = pageSize, CurrentPage = pageNumber,
            };
        }
    }
}
