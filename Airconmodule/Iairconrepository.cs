using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Main;

namespace Api.Airconmodule
{
    public interface IAirconRepository
    {
        Task<IEnumerable<AirconUnit>> GetAllAsync();
        Task<IEnumerable<AirconUnit>> GetByCustomerIdAsync(int customerId);
        Task<AirconUnit?> GetByIdAsync(int unitId);
        Task AddAsync(AirconUnit entity);
        Task UpdateAsync(AirconUnit entity);
        Task DeleteAsync(int unitId);
        Task<IEnumerable<AirconUnit>> SearchAsync(string query);
        Task<PaginationModel<AirconUnit>> GetPaginatedAsync(int pageNumber, int pageSize);
    }
}