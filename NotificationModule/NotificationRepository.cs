using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Api.Main;

namespace Api.NotificationModule
{
    public class NotificationRepository : BaseRepository, INotificationRepository
    {
        public NotificationRepository(MyCon dbConnection) : base(dbConnection) { }

        private const string SelectCols =
            "notification_id, user_id, title, message, is_read, created_at";

        private Notification MapReader(DbDataReader r) => new Notification
        {
            NotificationId = ReadValue<int>(r,    "notification_id", 0),
            UserId         = ReadValue<int>(r,    "user_id",         0),
            Title          = ReadValue<string>(r, "title",           string.Empty),
            Message        = ReadValue<string>(r, "message",         string.Empty),
            IsRead         = ReadValue<int>(r,    "is_read",         0) == 1,
            CreatedAt      = ReadValue<string>(r, "created_at",      string.Empty),
        };

        public async Task<IEnumerable<Notification>> GetByUserIdAsync(int userId)
        {
            string sql = $"SELECT {SelectCols} FROM notifications WHERE user_id = @uid ORDER BY created_at DESC";
            return await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("uid", userId) });
        }

        public async Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(int userId)
        {
            string sql = $"SELECT {SelectCols} FROM notifications WHERE user_id = @uid AND is_read = 0 ORDER BY created_at DESC";
            return await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("uid", userId) });
        }

        public async Task<Notification?> GetByIdAsync(int notificationId)
        {
            string sql = $"SELECT {SelectCols} FROM notifications WHERE notification_id = @nid";
            var list = await ExecuteReaderToListAsync(sql, MapReader,
                new[] { CreateParameter("nid", notificationId) });
            return list.FirstOrDefault();
        }

        public async Task AddAsync(Notification entity)
        {
            const string sql = @"
                INSERT INTO notifications (user_id, title, message, is_read)
                VALUES (@userId, @title, @message, 0)";
            await ExecuteNonQueryAsync(sql, new[]
            {
                CreateParameter("userId",  entity.UserId),
                CreateParameter("title",   (object?)entity.Title   ?? DBNull.Value),
                CreateParameter("message", (object?)entity.Message ?? DBNull.Value),
            });
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            const string sql = "UPDATE notifications SET is_read = 1 WHERE notification_id = @nid";
            await ExecuteNonQueryAsync(sql, new[] { CreateParameter("nid", notificationId) });
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            const string sql = "UPDATE notifications SET is_read = 1 WHERE user_id = @uid";
            await ExecuteNonQueryAsync(sql, new[] { CreateParameter("uid", userId) });
        }

        public async Task DeleteAsync(int notificationId)
        {
            const string sql = "DELETE FROM notifications WHERE notification_id = @nid";
            await ExecuteNonQueryAsync(sql, new[] { CreateParameter("nid", notificationId) });
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            const string sql = "SELECT COUNT(*) FROM notifications WHERE user_id = @uid AND is_read = 0";
            return (await ExecuteScalarAsync<int?>(sql, new[] { CreateParameter("uid", userId) })) ?? 0;
        }
    }
}
