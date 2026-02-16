using Microsoft.AspNetCore.SignalR;

namespace Clinic_System.API.Hubs
{
    public class ClinicHub:Hub
    {
        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            Console.WriteLine($"---> User Connected: {connectionId}");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"---> Connection Lost: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
