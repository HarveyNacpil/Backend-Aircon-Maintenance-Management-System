using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Api.Main;

namespace Api.UserModules
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        public UserRepository(MyCon dbConnection) : base(dbConnection) { }

        private User MapReader(DbDataReader reader) => new User
        {
            UserId     = ReadValue<int>(reader,    "user_id",    0),
            FullName   = ReadValue<string>(reader, "full_name",  string.Empty),
            Username   = ReadValue<string>(reader, "username",   string.Empty),
            Password   = ReadValue<string>(reader, "password",   string.Empty),
            Role       = ReadValue<string>(reader, "role",       "Staff"),
            Phone      = ReadValue<string>(reader, "phone",      string.Empty),
            Email      = ReadValue<string>(reader, "email",      string.Empty),
            Status     = ReadValue<string>(reader, "status",     "Active"),
            CreatedAt  = ReadValue<string>(reader, "created_at", string.Empty),
        };

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            const string sql = @"
                SELECT user_id, full_name, username, password, role, phone, email, status, created_at
                FROM users
                ORDER BY user_id";
            return await ExecuteReaderToListAsync(sql, MapReader);
        }

        public async Task<User?> GetByIdAsync(int userId)
        {
            const string sql = @"
                SELECT user_id, full_name, username, password, role, phone, email, status, created_at
                FROM users WHERE user_id = @userId";
            var list = await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("userId", userId) });
            return list.FirstOrDefault();
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            const string sql = @"
                SELECT user_id, full_name, username, password, role, phone, email, status, created_at
                FROM users WHERE username = @username";
            var list = await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("username", username) });
            return list.FirstOrDefault();
        }

        public async Task<User?> LoginAsync(string username, string password)
        {
            const string sql = @"
                SELECT user_id, full_name, username, password, role, phone, email, status, created_at
                FROM users WHERE username = @username AND password = @password AND status = 'Active'";
            var list = await ExecuteReaderToListAsync(sql, MapReader, new[]
            {
                CreateParameter("username", username),
                CreateParameter("password", password)
            });
            return list.FirstOrDefault();
        }

        public async Task AddAsync(User entity)
        {
            const string sql = @"
                INSERT INTO users (full_name, username, password, role, phone, email, status)
                VALUES (@fullName, @username, @password, @role, @phone, @email, @status)";
            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("fullName", entity.FullName),
                CreateParameter("username", entity.Username),
                CreateParameter("password", entity.Password),
                CreateParameter("role",     entity.Role),
                CreateParameter("phone",    (object?)entity.Phone ?? DBNull.Value),
                CreateParameter("email",    (object?)entity.Email ?? DBNull.Value),
                CreateParameter("status",   entity.Status),
            });
        }

        public async Task UpdateAsync(User entity)
        {
            const string sql = @"
                UPDATE users
                SET full_name = @fullName,
                    username  = @username,
                    password  = @password,
                    role      = @role,
                    phone     = @phone,
                    email     = @email,
                    status    = @status
                WHERE user_id = @userId";
            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("userId",   entity.UserId),
                CreateParameter("fullName", entity.FullName),
                CreateParameter("username", entity.Username),
                CreateParameter("password", entity.Password),
                CreateParameter("role",     entity.Role),
                CreateParameter("phone",    (object?)entity.Phone ?? DBNull.Value),
                CreateParameter("email",    (object?)entity.Email ?? DBNull.Value),
                CreateParameter("status",   entity.Status),
            });
        }

        public async Task DeleteAsync(int userId)
        {
            const string sql = "DELETE FROM users WHERE user_id = @userId";
            await ExecuteNonQueryAsync(sql, new[] { CreateParameter("userId", userId) });
        }

        public async Task<IEnumerable<User>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return await GetAllAsync();
            const string sql = @"
                SELECT user_id, full_name, username, password, role, phone, email, status, created_at
                FROM users
                WHERE full_name LIKE @q OR username LIKE @q OR role LIKE @q OR email LIKE @q";
            return await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("q", "%" + query + "%") });
        }

        public async Task<PaginationModel<User>> GetPaginatedAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize   <= 0) pageSize   = 10;

            const string sql = @"
                SELECT user_id, full_name, username, password, role, phone, email, status, created_at
                FROM users
                ORDER BY user_id
                LIMIT @size OFFSET @offset";

            var items = await ExecuteReaderToListAsync(sql, MapReader, new[]
            {
                CreateParameter("size",   pageSize),
                CreateParameter("offset", (pageNumber - 1) * pageSize),
            });

            int total = (await ExecuteScalarAsync<int?>("SELECT COUNT(*) FROM users")) ?? 0;
            return new PaginationModel<User>
            {
                Items       = items,
                TotalCount  = total,
                PageSize    = pageSize,
                CurrentPage = pageNumber,
            };
        }
    }
}