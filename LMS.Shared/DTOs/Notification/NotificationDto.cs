using Domain.Models.Entities;

namespace LMS.Shared.DTOs.Notification;

public class NotificationDto
{
	public int Id { get; set; }

	public NotificationType Type { get; set; }

	public DateTime CreatedAt { get; set; }
	public bool IsRead { get; set; }

	public string? ActorName { get; set; }

	public string? CourseName { get; set; }
	public string? ModuleName { get; set; }
	public string? ActivityName { get; set; }

	public string? DocumentName { get; set; }

	public int? SubmissionId { get; set; }

	public string? Message { get; set; }
}