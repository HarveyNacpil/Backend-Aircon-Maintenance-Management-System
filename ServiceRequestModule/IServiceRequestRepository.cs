using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Main;

namespace Api.ServiceRequestModule
{
    public interface IServiceRequestRepository
    {
        Task<IEnumerable<ServiceRequest>> GetAllAsync();
        Task<ServiceRequest?> GetByIdAsync(int requestId);
        Task<IEnumerable<ServiceRequest>> GetByCustomerAsync(int customerId);
        Task<IEnumerable<ServiceRequest>> GetByTechnicianAsync(int technicianId);
        Task<IEnumerable<ServiceRequest>> GetByStatusAsync(string status);
        Task AddAsync(ServiceRequest entity);
        Task UpdateAsync(ServiceRequest entity);
        Task AssignTechnicianAsync(int requestId, int technicianId);
        Task UpdateStatusAsync(int requestId, string status);
        Task DeleteAsync(int requestId);
        Task<PaginationModel<ServiceRequest>> GetPaginatedAsync(int pageNumber, int pageSize);
    }
}
