using ECO.BLL.DTO.Notification;

namespace ECO.BLL.Services.Notifications
{
    public interface INotificationService
    {
        Task CreateAsync(
            string recipientEmail,
            string title,
            string message,
            string type = "General",
            int? relatedEntityId = null,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<NotificationDto>> GetForUserAsync(
            string recipientEmail,
            CancellationToken cancellationToken = default);

        Task<int> GetUnreadCountAsync(
            string recipientEmail,
            CancellationToken cancellationToken = default);

        Task<bool> MarkAsReadAsync(
            int id,
            string recipientEmail,
            CancellationToken cancellationToken = default);

        Task<int> ClearForUserAsync(
            string recipientEmail,
            CancellationToken cancellationToken = default);
    }
}
