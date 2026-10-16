using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.StoreModule
{
    public interface IStoreRepository
    {
        Task<IEnumerable<Store>> GetAllAsync();
        Task<Store?> GetByIdAsync(int storeId);
        Task<int> AddAsync(Store entity);
        Task UpdateAsync(Store entity);
        Task<StoreDeleteCheckResult> CheckCanDeleteAsync(int storeId);
        Task DeleteAsync(int storeId);
        Task ReassignAndDeleteAsync(int sourceStoreId, int targetStoreId);
        Task SetMainBranchAsync(int storeId);
    }
}
