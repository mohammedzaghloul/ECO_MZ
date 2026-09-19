using ECO.Api.Helper;
using ECO.BLL.DTO.Notification;
using ECO.BLL.Services.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECO.Api.Controller
{
    [Authorize]
    public class NotificationController : BaseController
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var email = GetEmail();
            var notifications = await _notificationService.GetForUserAsync(email, cancellationToken);
            return Ok(new GenericResponseApi<IReadOnlyList<NotificationDto>>(
                200, "Notifications", notifications));
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
        {
            var email = GetEmail();
            var count = await _notificationService.GetUnreadCountAsync(email, cancellationToken);
            return Ok(new GenericResponseApi<int>(200, "Unread notifications", count));
        }

        [HttpPost("{id:int}/read")]
        public async Task<IActionResult> MarkAsRead(int id, CancellationToken cancellationToken)
        {
            var email = GetEmail();
            var marked = await _notificationService.MarkAsReadAsync(id, email, cancellationToken);
            return marked
                ? Ok(new ResponseApi(200, "Notification marked as read"))
                : NotFound(new ResponseApi(404, "Notification not found"));
        }

        [HttpDelete]
        public async Task<IActionResult> Clear(CancellationToken cancellationToken)
        {
            var email = GetEmail();
            var deleted = await _notificationService.ClearForUserAsync(email, cancellationToken);
            return Ok(new GenericResponseApi<int>(200, "Notifications cleared", deleted));
        }

        private string GetEmail()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(email))
                throw new UnauthorizedAccessException("A valid authenticated user is required.");
            return email;
        }
    }
}
