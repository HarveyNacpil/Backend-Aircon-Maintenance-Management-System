using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Main;

namespace Api.Maintenance
{
    public interface IMaintenanceRepository
    {
        // --- Schedules ---
        Task<IEnumerable<MaintenanceSchedule>> GetAllSchedulesAsync();
        Task<MaintenanceSchedule?> GetScheduleByIdAsync(int scheduleId);
        Task<IEnumerable<MaintenanceSchedule>> GetSchedulesByCustomerAsync(int customerId);
        Task<IEnumerable<MaintenanceSchedule>> GetSchedulesByTechnicianAsync(int technicianId);
        Task AddScheduleAsync(MaintenanceSchedule entity);
        Task UpdateScheduleAsync(MaintenanceSchedule entity);
        Task DeleteScheduleAsync(int scheduleId);
        Task<PaginationModel<MaintenanceSchedule>> GetSchedulesPaginatedAsync(int pageNumber, int pageSize);

        // --- Records ---
        Task<IEnumerable<MaintenanceRecord>> GetAllRecordsAsync();
        Task<MaintenanceRecord?> GetRecordByIdAsync(int recordId);
        Task<MaintenanceRecord?> GetRecordByScheduleIdAsync(int scheduleId);
        Task AddRecordAsync(MaintenanceRecord entity);
        Task UpdateRecordAsync(MaintenanceRecord entity);
        Task DeleteRecordAsync(int recordId);
        Task<PaginationModel<MaintenanceRecord>> GetRecordsPaginatedAsync(int pageNumber, int pageSize);
    }
}
