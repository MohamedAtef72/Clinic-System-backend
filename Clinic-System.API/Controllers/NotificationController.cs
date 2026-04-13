using Clinic_System.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_System.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationQueryService _notificationService;

        public NotificationController(INotificationQueryService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("User")]
        public async Task<IActionResult> GetUserNotifications()
        {
            var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var pageNumberStr = HttpContext.Request.Query["pageNumber"].FirstOrDefault();

            int pageNumber = 1;
            int pageSize = 6;

            if (!string.IsNullOrEmpty(pageNumberStr) && int.TryParse(pageNumberStr, out var p))
                pageNumber = Math.Max(1, p);

            var (notifications , totalCount) = await _notificationService.GetUserNotificationsAsync(userId, pageNumber, pageSize); 

            return Ok(new { Message = "Notifications retrieved successfully", PageNumber = pageNumber, PageSize = pageSize, TotalCount = totalCount, Data = notifications });
        }

        [HttpPost("MarkAllAsRead")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(new { Message = "All notifications marked as read." });
        }

        [HttpPost("MarkAsRead/{notificationId}")]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _notificationService.MarkNotificationAsReadAsync(userId, notificationId);
            return Ok(new { Message = "Notification marked as read." });
        }
    }
}