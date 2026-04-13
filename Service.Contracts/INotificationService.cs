

using LMS.Shared.DTOs.Notification;

namespace Service.Contracts;

public interface INotificationService
{
	Task<IEnumerable<NotificationDto>> GetUnreadByUserIdAsync(string userId);
	Task MarkAsReadAsync(int notificationId, string userId);
}
