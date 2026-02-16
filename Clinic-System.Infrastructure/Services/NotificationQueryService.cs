using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Repositories;

namespace Clinic_System.Infrastructure.Services
{
    public class NotificationQueryService : INotificationQueryService
    {
        private readonly NotificationRepository _repo;

        public NotificationQueryService(NotificationRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<NotificationDto>> GetUserNotificationsAsync(string userId, int pageNumber, int pageSize)
        {
            return await _repo.GetUserNotificationsAsync(userId, pageNumber, pageSize);
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            // This line marks all notifications as read for the user
            await _repo.MarkAllAsReadAsync(userId);
        }

        public async Task MarkNotificationAsReadAsync(string userId, int notificationId)
        {
            await _repo.MarkNotificationAsReadAsync(userId, notificationId);
        }

        public async Task CreateNotificationForUserAsync(string userId, Notification notification)
        {
            // ensure notification saved
            await _repo.AddAsync(notification);
            var userNotification = new UserNotification
            {
                UserId = userId,
                NotificationId = notification.Id,
                IsRead = false
            };
            await _repo.AddUserNotificationAsync(userNotification);
        }

        public async Task CreateGlobalNotificationAsync(Notification notification)
        {
            notification.IsGlobal = true;
            await _repo.AddAsync(notification);

            // Create user notifications for all users is expensive here; typically skipped and clients read global notifications by querying notifications where IsGlobal
        }
    }
}