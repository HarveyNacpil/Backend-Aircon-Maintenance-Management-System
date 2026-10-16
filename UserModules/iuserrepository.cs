using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Main;

namespace Api.UserModules
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int userId);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> LoginAsync(string username, string password);
        Task AddAsync(User entity);
        Task UpdateAsync(User entity);
        Task DeleteAsync(int userId);
        Task<IEnumerable<User>> SearchAsync(string query);
        Task<PaginationModel<User>> GetPaginatedAsync(int pageNumber, int pageSize);
    }
}