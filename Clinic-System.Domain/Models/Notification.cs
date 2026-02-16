using System;
using System.ComponentModel.DataAnnotations;

namespace Clinic_System.Domain.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsGlobal { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<UserNotification> UserNotifications { get; set; }
    }
}
