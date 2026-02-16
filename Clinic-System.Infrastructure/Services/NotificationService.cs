using Clinic_System.API.Hubs;
using Clinic_System.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<ClinicHub> _hubContext;

        public NotificationService(IHubContext<ClinicHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendNotificationToAll(string title, string message, string type = "General")
        {
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
            {
                Title = title,
                Message = message,
                Type = type, 
                SentAt = DateTime.Now
            });
        }

        public async Task SendNotificationToUser(string userId, string title, string message, string type = "Personal")
        {
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", new
            {
                Title = title,
                Message = message,
                Type = type, 
                SentAt = DateTime.Now
            });
        }
    }
}
