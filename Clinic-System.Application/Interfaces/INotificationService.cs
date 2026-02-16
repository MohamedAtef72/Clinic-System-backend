using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.Interfaces
{
    public interface INotificationService
    {
        Task SendNotificationToAll(string title, string message, string type);

        Task SendNotificationToUser(string userId, string title, string message, string type);
    }
}
