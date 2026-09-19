using ECO.BLL.DTO.Notification;
using ECO.DAL.Data;
using ECO.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECO.BLL.Services.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _dbContext;

        public NotificationService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task CreateAsync(
            string recipientEmail,
            string title,
            string message,
            string type = "General",
            int? relatedEntityId = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(recipientEmail))
                throw new ArgumentException("Recipient email is required", nameof(recipientEmail));

            _dbContext.Notifications.Add(new Notification
            {
                RecipientEmail = recipientEmail.Trim(),
                Title = title.Trim(),
                Message = message.Trim(),
                Type = string.IsNullOrWhiteSpace(type) ? "General" : type.Trim(),
                RelatedEntityId = relatedEntityId
            });
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<NotificationDto>> GetForUserAsync(
            string recipientEmail,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Notifications
                .AsNoTracking()
                .Where(notification => notification.RecipientEmail == recipientEmail)
                .OrderByDescending(notification => notification.CreatedAt)
                .Take(50)
                .Select(notification => new NotificationDto
                {
                    Id = notification.Id,
                    Title = notification.Title,
                    Message = notification.Message,
                    Type = notification.Type,
                    RelatedEntityId = notification.RelatedEntityId,
                    IsRead = notification.IsRead,
                    CreatedAt = notification.CreatedAt
                })
                .ToListAsync(cancellationToken);
        }

        public Task<int> GetUnreadCountAsync(
            string recipientEmail,
            CancellationToken cancellationToken = default)
        {
            return _dbContext.Notifications
                .CountAsync(notification =>
                    notification.RecipientEmail == recipientEmail && !notification.IsRead,
                    cancellationToken);
        }

        public async Task<bool> MarkAsReadAsync(
            int id,
            string recipientEmail,
            CancellationToken cancellationToken = default)
        {
            var notification = await _dbContext.Notifications
                .FirstOrDefaultAsync(item =>
                    item.Id == id && item.RecipientEmail == recipientEmail,
                    cancellationToken);

            if (notification is null)
                return false;

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return true;
        }

        public async Task<int> ClearForUserAsync(
            string recipientEmail,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(recipientEmail))
                throw new ArgumentException("Recipient email is required", nameof(recipientEmail));

            var notifications = await _dbContext.Notifications
                .Where(notification => notification.RecipientEmail == recipientEmail.Trim())
                .ToListAsync(cancellationToken);

            if (notifications.Count == 0)
                return 0;

            _dbContext.Notifications.RemoveRange(notifications);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return notifications.Count;
        }
    }
}
