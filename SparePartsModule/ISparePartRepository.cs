using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Main;

namespace Api.SparePartsModule
{
    public interface ISparePartRepository
    {
        // Spare Parts
        Task<IEnumerable<SparePart>> GetAllPartsAsync();
        Task<SparePart?> GetPartByIdAsync(int partId);
        Task AddPartAsync(SparePart entity);
        Task UpdatePartAsync(SparePart entity);
        Task DeletePartAsync(int partId);
        Task<IEnumerable<SparePart>> SearchPartsAsync(string query);
        Task<PaginationModel<SparePart>> GetPartsPaginatedAsync(int pageNumber, int pageSize);

        // Parts Usage
        Task<IEnumerable<PartUsage>> GetUsageByRecordAsync(int recordId);
        Task AddUsageAsync(PartUsage entity);
        Task DeleteUsageAsync(int usageId);
    }
}
