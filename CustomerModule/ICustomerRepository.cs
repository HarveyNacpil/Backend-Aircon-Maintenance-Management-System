using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Main;

namespace Api.CustomerModule
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<Customer>> GetAllAsync();
        Task<Customer?> GetByIdAsync(int customerId);
        Task AddAsync(Customer entity);
        Task UpdateAsync(Customer entity);
        Task DeleteAsync(int customerId);
        Task<IEnumerable<Customer>> SearchAsync(string query);
        Task<PaginationModel<Customer>> GetPaginatedAsync(int pageNumber, int pageSize);
        Task<IEnumerable<CustomerMapItem>> GetCustomerMapLocationsAsync();
        Task UpdateCoordinatesAsync(int customerId, double latitude, double longitude);
    }
}
