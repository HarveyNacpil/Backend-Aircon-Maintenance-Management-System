using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Main;

namespace Api.NotificationModule
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Notification>> GetUnreadByUserIdAsync(int userId);
        Task<Notification?> GetByIdAsync(int notificationId);
        Task AddAsync(Notification entity);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(int userId);
        Task DeleteAsync(int notificationId);
        Task<int> GetUnreadCountAsync(int userId);
    }
}
