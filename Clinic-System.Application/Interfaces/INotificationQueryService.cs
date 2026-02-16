using Clinic_System.Application.DTO;
using Clinic_System.Domain.Models;

namespace Clinic_System.Application.Interfaces
{
    public interface INotificationQueryService
    {
        Task<List<NotificationDto>> GetUserNotificationsAsync(string userId, int pageNumber, int pageSize);
        Task MarkAllAsReadAsync(string userId);
        Task MarkNotificationAsReadAsync(string userId, int notificationId);
        Task CreateNotificationForUserAsync(string userId, Clinic_System.Domain.Models.Notification notification);
        Task CreateGlobalNotificationAsync(Clinic_System.Domain.Models.Notification notification);
    }
}