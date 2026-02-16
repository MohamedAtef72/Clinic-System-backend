using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Clinic_System.Domain.Models
{
    public class UserNotification    
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }

        public int NotificationId { get; set; }  

        [ForeignKey(nameof(NotificationId))]   
        public Notification Notification { get; set; }

        public bool IsRead { get; set; }
    }
}