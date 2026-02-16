using Clinic_System.Application.DTO;
using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Clinic_System.Infrastructure.Repositories
{
    public class NotificationRepository
    {
        private readonly AppDbContext _db;

        public NotificationRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Notification notification)
        {
            await _db.Notifications.AddAsync(notification);
            await _db.SaveChangesAsync();
        }

        public async Task AddUserNotificationAsync(UserNotification userNotification)
        {
            await _db.UserNotifications.AddAsync(userNotification);
            await _db.SaveChangesAsync();
        }

        public async Task<List<NotificationDto>> GetUserNotificationsAsync(string userId, int pageNumber, int pageSize)
        {
            // LEFT JOIN Notifications with UserNotifications to include global and personal notifications
            var query = from n in _db.Notifications
                        join un in _db.UserNotifications.Where(x => x.UserId == userId)
                            on n.Id equals un.NotificationId into gj
                        from sub in gj.DefaultIfEmpty()
                        where n.IsGlobal || sub != null
                        orderby n.CreatedAt descending
                        select new { Notification = n, UserNotification = sub };

            var paged = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = paged.Select(x => new NotificationDto
            {
                Id = x.Notification.Id,
                Title = x.Notification.Title,
                Message = x.Notification.Message,
                IsRead = x.UserNotification?.IsRead ?? false,
                CreatedAt = x.Notification.CreatedAt
            }).ToList();

            return result;
        }


        public async Task MarkAllAsReadAsync(string userId)
        {
            // Mark existing user notifications as read
            var userNotifications = await _db.UserNotifications.Where(un => un.UserId == userId && !un.IsRead).ToListAsync();
            foreach (var n in userNotifications)
                n.IsRead = true;

            // For global notifications that don't have a UserNotification yet, create UserNotification with IsRead = true
            var globalNotifications = await _db.Notifications
                .Where(n => n.IsGlobal)
                .ToListAsync();

            var existingNotificationIds = new HashSet<int>(await _db.UserNotifications.Where(un => un.UserId == userId).Select(un => un.NotificationId).ToListAsync());

            var toCreate = globalNotifications
                .Where(n => !existingNotificationIds.Contains(n.Id))
                .Select(n => new UserNotification
                {
                    UserId = userId,
                    NotificationId = n.Id,
                    IsRead = true
                })
                .ToList();

            if (toCreate.Any())
            {
                await _db.UserNotifications.AddRangeAsync(toCreate);
            }

            await _db.SaveChangesAsync();
        }

        public async Task MarkNotificationAsReadAsync(string userId, int notificationId)
        {
            // Check if a UserNotification exists for this user & notification
            var userNotification = await _db.UserNotifications
                .FirstOrDefaultAsync(un => un.UserId == userId && un.NotificationId == notificationId);

            if (userNotification != null)
            {
                if (!userNotification.IsRead)
                {
                    userNotification.IsRead = true;
                    await _db.SaveChangesAsync();
                }
                return;
            }

            // If no UserNotification exists, this could be a global notification. Create a read record.
            var notification = await _db.Notifications.FindAsync(notificationId);
            if (notification == null) return; // nothing to do

            var newUserNotification = new UserNotification
            {
                UserId = userId,
                NotificationId = notificationId,
                IsRead = true
            };

            await _db.UserNotifications.AddAsync(newUserNotification);
            await _db.SaveChangesAsync();
        }
    }
}