using Clinic_System.Application.DTO;
using Clinic_System.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.Interfaces
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification);
        Task AddUserNotificationAsync(UserNotification userNotification);
        Task<(List<NotificationDto> Notifications, int TotalCount)> GetUserNotificationsAsync(string userId, int pageNumber, int pageSize);
        Task MarkAllAsReadAsync(string userId);
        Task MarkNotificationAsReadAsync(string userId, int notificationId);
    }
}
